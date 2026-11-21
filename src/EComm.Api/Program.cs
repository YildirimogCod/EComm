using EComm.Basket;
using EComm.Catalog;
using EComm.Inventory;
using EComm.Ordering;
using EComm.Payment;

using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddBasketModule();
builder.Services.AddCatalogModule();
builder.Services.AddInventoryModule();
builder.Services.AddOrderingModule();
builder.Services.AddPaymentModule();

var app = builder.Build();




if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();


app.Run();


