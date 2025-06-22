using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using global::OrderService.Application.DTOs;
using global::OrderService.Application.Interfaces;
using global::OrderService.Application.UseCases;
using Moq;
using OrderService.Domain.Entities;
using Xunit;

namespace OrderService.Tests.UseCases
{
  


    public class CreateOrderUseCaseTests
    {
        [Fact]
        public async Task ExecuteAsync_DeveCriarPedidoQuandoNaoExiste()
        {
            // Arrange
            var orderRepositoryMock = new Mock<IOrderRepository>();
            orderRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                               .ReturnsAsync(false);

            var useCase = new CreateOrderUseCase(orderRepositoryMock.Object);

            var dto = new CreateOrderDto
            {
                ExternalOrderId = "EX123",
                Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Produto A", Quantity = 2, UnitPrice = 10.0m },
                new() { ProductName = "Produto B", Quantity = 1, UnitPrice = 5.5m }
            }
            };

            // Act
            await useCase.ExecuteAsync(dto);

            // Assert
            orderRepositoryMock.Verify(r => r.SaveAsync(It.Is<Order>(o =>
                o.ExternalOrderId == "EX123" &&
                o.Items.Count == 2 &&
                o.Total == 25.5m
            )), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_DeveIgnorarPedidoDuplicado()
        {
            // Arrange
            var orderRepositoryMock = new Mock<IOrderRepository>();
            orderRepositoryMock.Setup(x => x.ExistsAsync("EX456"))
                               .ReturnsAsync(true);

            var useCase = new CreateOrderUseCase(orderRepositoryMock.Object);

            var dto = new CreateOrderDto
            {
                ExternalOrderId = "EX456",
                Items = new List<CreateOrderItemDto>
            {
                new() { ProductName = "Produto X", Quantity = 1, UnitPrice = 100 }
            }
            };

            // Act
            await useCase.ExecuteAsync(dto);

            // Assert
            orderRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Never);
        }
    }

}
