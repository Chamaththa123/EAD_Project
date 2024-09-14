import { createBrowserRouter } from "react-router-dom";
import { MainLayout } from "../components/layouts/MainLayout";
import { GuestLayout } from "../components/layouts/GuestLayout";
import { Dashboard } from "../pages/Dashboard/Dashboard";

const router = createBrowserRouter([
  {
    path: "/",
    element: <MainLayout />,
    children: [
      { path: "/", 
        element: <Dashboard /> 
    },
    //   {
    //     path: "/estates",
    //     element: <Estates />,
    //   },
    ],
  },
  // {
  //   path: "/",
  //   element: <GuestLayout />,
  //   children: [
  //     {
  //       path: "/login",
  //       element: <Dashboard /> 
  //     },
  //     {
  //       path: "/signup",
  //       // element: <SignUp />,
  //     },
  //   ],
  // },
]);

export default router;