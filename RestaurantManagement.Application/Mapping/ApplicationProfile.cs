using AutoMapper;
using RestaurantManagement.Application.DTOs.Categories;
using RestaurantManagement.Application.DTOs.Customers;
using RestaurantManagement.Application.DTOs.Inventory;
using RestaurantManagement.Application.DTOs.Orders;
using RestaurantManagement.Application.DTOs.POS;
using RestaurantManagement.Application.DTOs.PurchaseOrders;
using RestaurantManagement.Application.DTOs.Products;
using RestaurantManagement.Application.DTOs.Reservations;
using RestaurantManagement.Application.DTOs.SalesOrders;
using RestaurantManagement.Application.DTOs.Suppliers;
using RestaurantManagement.Application.DTOs.Tables;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Mapping;

public sealed class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<Category, UpdateCategoryDto>();
        CreateMap<UpdateCategoryDto, Category>();

        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<Product, UpdateProductDto>();
        CreateMap<UpdateProductDto, Product>();

        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateSupplierDto, Supplier>();
        CreateMap<Supplier, UpdateSupplierDto>();
        CreateMap<UpdateSupplierDto, Supplier>();

        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<Customer, UpdateCustomerDto>();
        CreateMap<UpdateCustomerDto, Customer>();

        CreateMap<RestaurantTable, TableDto>();
        CreateMap<CreateTableDto, RestaurantTable>();
        CreateMap<RestaurantTable, UpdateTableDto>();
        CreateMap<UpdateTableDto, RestaurantTable>();

        CreateMap<Reservation, ReservationDto>()
            .ForMember(d => d.CustomerName, m => m.MapFrom(s => s.Customer != null ? s.Customer.Name : string.Empty))
            .ForMember(d => d.TableNumber, m => m.MapFrom(s => s.Table != null ? s.Table.TableNumber : null));

        CreateMap<Reservation, UpdateReservationDto>();
        CreateMap<UpdateReservationDto, Reservation>();

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, m => m.MapFrom(s => s.Product != null ? s.Product.NameEn : string.Empty));

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName, m => m.MapFrom(s => s.Customer != null ? s.Customer.Name : null))
            .ForMember(d => d.TableNumber, m => m.MapFrom(s => s.Table != null ? s.Table.TableNumber : null));

        CreateMap<Order, UpdateOrderDto>();
        CreateMap<OrderItem, UpdateOrderItemDto>();

        CreateMap<SalesOrder, SalesOrderListItemDto>();
        CreateMap<PurchaseOrder, PurchaseOrderListItemDto>();

        CreateMap<InventoryMovement, InventoryMovementDto>()
            .ForMember(d => d.ProductName, m => m.MapFrom(s => s.Product != null ? s.Product.NameEn : string.Empty));

        CreateMap<Product, PosProductDto>();
    }
}
