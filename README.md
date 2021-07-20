# Description
Small Shop REST API made in ASP NET Core for exercise.

**Features:**

 - RESTful routing
 - Stateless communication
 - Filtering/Paging/Sorting 
 - Basic header authorization

# Routes
*All requests must be authorized with [basic header authorization](https://en.wikipedia.org/wiki/Basic_access_authentication), example data is stored in accounts.txt*

Base URL: **shop_api/**

**Products**
|Method|Route|Action|Description| 
|---|---|---|---|
|GET|products|ProductsController::GetProducts|Gets all products of user, can be filtered/sorted/paged.
|GET|products/{id}|ProductsController::GetProduct|Gets product with id.
|POST|products|ProductsController::CreateProduct|Creates new product.
|DELETE|products/{id}|ProductsController::DeleteProduct|Deletes existing product.
|PUT|products/{id}|ProductsController::UpdateProduct|Updates existing product.
|POST|products/{id}/images|ProductsController::AddImageToProduct|Adds product image, as an url to earlier uploaded image.
|GET|products/{id}/images|ProductsController::GetProductImages|Gets all product images.
|DELETE|products/{id}/images|ProductsController::DeleteProductImage|Deletes product Image.

**Categories**
|Method|Route|Action|Description| 
|---|---|---|---|
|GET|categories|CategoriesController::GetCategories|Gets all categories

**Orders**
|Method|Route|Action|Description| 
|---|---|---|---|
|GET|orders|OrdersController::GetOrders|Gets all orders of user, can be filtered/sorted/paged.
|GET|orders/{id}|OrdersController::GetOrder|Get order with id.
|POST|orders|OrdersController::CreateOrder|Creates new order.
|DELETE|orders/{id}|OrdersController::DeleteOrder|Deletes existing order.
|PUT|orders/{id}|OrdersController::UpdateOrder|Updates existing order.

**Users**
|Method|Route|Action|Description| 
|---|---|---|---|
|GET|user|UserController::GetUserData|Get data describing current user.

**Images**
|Method|Route|Action|Description| 
|---|---|---|---|
|GET|images/{id}|ImagesController::GetImage|Returns image file.
|POST|images/|ImagesController::CreateImage|Uploads new image.

*More details can be seen in swagger after building project and running ISS express. Swagger will not show all possible return codes.*

# Filtering
Example:

    GET shop_api/products?sortBy=Name&filter=Price>>20
Gets all users' products that have prive greater than 20, and sorts them by name property.

**Third party:**
 - Filtering/Paging/Sorting provided by [Gridify](https://alirezanet.github.io/Gridify/)
 - Password hashing is provided by [Bcrypt](https://github.com/BcryptNet/bcrypt.net)
