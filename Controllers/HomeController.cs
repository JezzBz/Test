using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Test.Models;

namespace Test.Controllers
{
    [Route("api")]
    [ApiController]
    public class HomeController : Controller
    {
        private static List<Product> Products = new List<Product> { new Product { Name = "Product1", Count = 1 }, new Product { Name = "Product2", Count = 2 }, new Product { Name = "Product3", Count = 3 } };
        private static List<Order> Orders = new List<Order> { };

        [HttpGet("products/getAll")]
        public IActionResult GetAllProducts() => Json(Products);
      
        [HttpPost("orders/create")]
        public IActionResult CreateOrder(string products)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<Product> OrderProducts = null;

            //Проверка на возможность десериализации
            try
            {
                OrderProducts = JsonSerializer.Deserialize<List<Product>>(products, options);
            }
            catch (Exception)
            {

                return Json("Неверный формат отправленных данных");
            }

            //Проверка валидности 
            foreach (var item in OrderProducts)
            {
                //Проверка на наличие несуществующих товаров
                if (!Products.Any(x => x.Name == item.Name))
                {
                    return NotFound(Json($"Неизвестный товар {item.Name}"));
                }
                //Проверка на соответствие кол-ва товаров
                Product product = Products.First(x => x.Name == item.Name);
                if (item.Count <= 0)
                {
                    return BadRequest(Json("Количество товарад должно быть больше  0"));
                }
                if (item.Count > product.Count)
                {

                    return BadRequest(Json($"Количество товара'{item.Name}': {product.Count}"));
                }
            }


            Order order = new Order { KeyNumber = Orders.Count() + 1, products = OrderProducts };
            //Изменение состояния базы товаров
            foreach (var item in OrderProducts)
            {
                Products.First(x => x.Name == item.Name).Count -= item.Count;
            }
           
            Orders.Add(order);

            return Ok(Json(order));

        }

        //Удаление заказа по keynumber
        [HttpDelete("orders/delete")]
        public IActionResult DeleteOrder(int key)
        {
            if (Orders.Any(x=>x.KeyNumber==key))
            {
                Order order = Orders.First(x => x.KeyNumber == key);
                Orders.Remove(order);
                //Возвращаем количество доступного товара
                foreach (var item in order.products )
                {
                    Products.First(x => x.Name == item.Name).Count += item.Count;
                }
                return Ok("Заказ успешно удалён!");
            }
            else
            {
                return NotFound("Заказ не найден");
            }
        }
        //Удаление товара из списка товаров
        [HttpDelete("products/delete")]
        public IActionResult DeleteProduct(string Name)
        {
            if (Products.Any(x=>x.Name==Name))
            {
                Products.Remove(Products.First(x => x.Name == Name));
                return Ok("Товар успешно удалён из списка доступных товаров!");
            }
            else
            {
                return NotFound("Товар не найден");
            }
        }
        //Удаление товара из заказа
        [HttpDelete("orders/deleteProduct")]
        public IActionResult DeleteProductFromOrder(int OrderKey, string ProductName)
        {
            if (!Orders.Any(x => x.KeyNumber == OrderKey))
            {
                return NotFound($"Заказ с номером {OrderKey} не найден!");
            }
            Order order = Orders.First(x => x.KeyNumber == OrderKey);
            if (!order.products.Any(x => x.Name == ProductName))
            {
                return NotFound($"Товар {ProductName} не найден в заказе");
            }
            if (Products.Any(x=>x.Name==ProductName))
            {
                Products.First(x => x.Name == ProductName).Count += order.products.First(x => x.Name == ProductName).Count;
            }
           
            order.products.Remove(order.products.First(x => x.Name == ProductName));
            if (order.products.Count==0)
            {
                Orders.Remove(order);
                return Json("После удаления товара заказ оказался пустым, он был удалён!");
            }
            return Json(order);

        } 

    }
}
