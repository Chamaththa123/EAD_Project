﻿/************************************************************
 * File:        IProductListService.cs
 * Author:      IT21210174 - Tharushi Lakshika V.G
 * Date:        2024-09-17
 * Description: This interface defines methods for managing product 
 *              lists in the WebService. It provides CRUD operations 
 *              and status management for product lists.
 ************************************************************/

using WebService.Models;
using static System.Net.Mime.MediaTypeNames;

namespace WebService.Interfaces
{
    // This interface provides methods for managing product list.

    public interface IProductListService
    {
        //get all product list
        Task<List<Product_List>> GetProductList();

        //get all active product list
        Task<List<Product_List>> GetActiveProductList();

        //get specific product list by id
        Task<Product_List?> GetProductListById(string id);

        //create new product list
        Task CreateProductList(Product_List newProductList);

        //update avilable productlist details
        Task UpdateProductList(string id, Product_List updatedProductList);

        //delete available productlist
        Task RemoveProductList(string id);

        //activate or deactivate productlist
        Task ChangeProductListStatus(string id);
    }
}
