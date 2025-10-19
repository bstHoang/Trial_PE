

-- Create the database
CREATE DATABASE ShopDB;
GO

-- Use the database
USE ShopDB;
GO

-- Create Products table
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    Quantity INT NOT NULL DEFAULT 0 CHECK (Quantity >= 0),
    Price DECIMAL(18,2) NOT NULL CHECK (Price > 0)
);
GO

-- Create Order table
CREATE TABLE [Order] (  -- Note: Order is a reserved word, so use [Order]
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATE NOT NULL DEFAULT GETDATE(),
    TotalPrice DECIMAL(18,2) NOT NULL CHECK (TotalPrice >= 0)
);
GO

-- Create OrderDetail table
CREATE TABLE OrderDetail (
    OrderDetailId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    Price DECIMAL(18,2) NOT NULL CHECK (Price > 0),
    FOREIGN KEY (OrderId) REFERENCES [Order](OrderId) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);
GO

-- Insert sample data into Products
INSERT INTO Products (ProductName, Quantity, Price) VALUES
(N'Product A', 10, 100.00),
(N'Product B', 20, 200.00),
(N'Product C', 5, 50.00),
(N'Product D', 15, 150.00);
GO

-- Insert sample data into Order and OrderDetail
-- First Order
INSERT INTO [Order] (OrderDate, TotalPrice) VALUES (GETDATE(), 300.00);
DECLARE @OrderId1 INT = SCOPE_IDENTITY();

INSERT INTO OrderDetail (OrderId, ProductId, Quantity, Price) VALUES
(@OrderId1, 1, 2, 100.00),  -- Sản phẩm A
(@OrderId1, 2, 1, 200.00);  -- Sản phẩm B

-- Second Order
INSERT INTO [Order] (OrderDate, TotalPrice) VALUES (GETDATE(), 100.00);
DECLARE @OrderId2 INT = SCOPE_IDENTITY();

INSERT INTO OrderDetail (OrderId, ProductId, Quantity, Price) VALUES
(@OrderId2, 3, 2, 50.00);  -- Sản phẩm C
GO

-- Update quantities in Products (simulate after orders)
UPDATE Products SET Quantity = Quantity - 2 WHERE ProductId = 1;  -- For Order 1
UPDATE Products SET Quantity = Quantity - 1 WHERE ProductId = 2;  -- For Order 1
UPDATE Products SET Quantity = Quantity - 2 WHERE ProductId = 3;  -- For Order 2
GO