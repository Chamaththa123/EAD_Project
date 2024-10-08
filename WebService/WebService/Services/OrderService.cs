﻿/************************************************************
 * File:        OrderService.cs
 * Author:      IT21252754 Madhumalka K.C.S
 * Date:        2024-09-24
 * Description: Implements the IOrderService interface for 
 *              managing orders in the application. This 
 *              includes creating orders, retrieving orders 
 *              by various criteria, updating orders, and 
 *              managing order cancellations.
 ************************************************************/


using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebService.Interfaces;
using WebService.Models;
using WebService.Settings;

namespace WebService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<User> _userCollection;

        public OrderService(IOptions<MongoDBSettings> mongoDBSettings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _orderCollection = database.GetCollection<Order>("order");
            _productCollection = database.GetCollection<Product>("product");
            _userCollection = database.GetCollection<User>("user");
        }

        // Create a new order
        public async Task<Order> CreateOrder(Order newOrder)
        {
            await _orderCollection.InsertOneAsync(newOrder);
            return newOrder;
        }

        // Get all orders
        public async Task<List<Order>> GetOrders()
        {
            var orders = await _orderCollection.Find(_ => true).ToListAsync();

            foreach (var order in orders)
            {
                var customer = await _userCollection.Find(c => c.Id == order.CustomerId).FirstOrDefaultAsync();
                if (customer != null)
                {
                    order.CustomerFirstName = customer.First_Name;
                    order.CustomerLastName = customer.Last_Name;
                }
            }

            return orders;
        }

        // Get order by ID
        public async Task<Order?> GetOrderById(string id)
        {
            var order = await _orderCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

            if (order == null)
            {
                return null;
            }

            if (order != null)
            {
                var customer = await _userCollection.Find(x => x.Id == order.CustomerId).FirstOrDefaultAsync();
                if (customer != null)
                {
                    order.CustomerFirstName = customer.First_Name;
                    order.CustomerLastName = customer.Last_Name;
                }

                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _productCollection.Find(p => p.Id == orderItem.ProductId).FirstOrDefaultAsync();
                    if (product != null)
                    {
                        orderItem.ProductName = product.Name;
                    }
                }
            }

            return order;
        }

        // Update an order by ID
        public async Task<Order?> UpdateOrder(string id, Order updatedOrder)
        {
            var result = await _orderCollection.ReplaceOneAsync(order => order.Id == id, updatedOrder);
            if (result.ModifiedCount > 0)
            {
                return updatedOrder;
            }
            return null;
        }

        // Request order cancellation
        public async Task<Order?> RequestOrderCancellation(string id, string cancellationNote)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.Id, id);
            var update = Builders<Order>.Update
                .Set(o => o.IsCancellationRequested, true)
                .Set(o => o.Status, 4)
                .Set(o => o.CancellationNote, cancellationNote);

            var result = await _orderCollection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Order>
            {
                ReturnDocument = ReturnDocument.After
            });

            return result;
        }

        // Approve order cancellation
        public async Task<Order?> ApproveOrderCancellation(string id)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.Id, id);
            var update = Builders<Order>.Update
                .Set(o => o.Status, 3) 
                .Set(o => o.IsCancellationApproved, 1);

            var result = await _orderCollection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Order>
            {
                ReturnDocument = ReturnDocument.After
            });

            return result;
        }

        // reject order cancellation
        public async Task<Order?> RejectOrderCancellation(string id)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.Id, id);
            var update = Builders<Order>.Update
                .Set(o => o.Status, 0)
                .Set(o => o.IsCancellationApproved, 2);

            var result = await _orderCollection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Order>
            {
                ReturnDocument = ReturnDocument.After
            });

            return result;
        }

        // Get all orders where cancellation has been requested but not yet approved
        public async Task<List<Order>> GetCancelRequests()
        {
            var filter = Builders<Order>.Filter.Where(o => o.IsCancellationRequested && o.IsCancellationApproved==1);
            return await _orderCollection.Find(filter).ToListAsync();
        }

        // Get all orders where cancellation has been approved
        public async Task<List<Order>> GetApprovedCancellations()
        {
            var filter = Builders<Order>.Filter.Where(o => o.IsCancellationRequested && o.IsCancellationApproved==2);
            return await _orderCollection.Find(filter).ToListAsync();
        }

        /// Retrieves all orders associated with a specific vendor ID.
        public async Task<List<Order>> GetOrdersByVendorId(string vendorId)
        {
            // Filter orders where any of the OrderItems have the given VendorId
            var filter = Builders<Order>.Filter.ElemMatch(order => order.OrderItems, item => item.VendorId == vendorId);
            var orders = await _orderCollection.Find(filter).ToListAsync();

            foreach (var order in orders)
            {
                var customer = await _userCollection.Find(c => c.Id == order.CustomerId).FirstOrDefaultAsync();
                if (customer != null)
                {
                    order.CustomerFirstName = customer.First_Name;
                    order.CustomerLastName = customer.Last_Name;
                }
            }

            return orders;
        }

        /// Retrieves all orders associated with a specific customer ID.
        public async Task<List<Order>> GetOrdersByCustromerId(string customerId)
        {
            // Filter orders by CustomerId
            var filter = Builders<Order>.Filter.Eq(order => order.CustomerId, customerId);
            var orders = await _orderCollection.Find(filter).ToListAsync();

            return orders;
        }

        // Get last order
        public async Task<Order?> GetLastOrder()
        {
            return await _orderCollection
                .Find(_ => true)
                .SortByDescending(o => o.OrderCode)
                .FirstOrDefaultAsync();
        }

    }
}
