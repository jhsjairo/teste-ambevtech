using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Xunit;

namespace OrderService.Tests.UseCases
{
    public class CreateOrderUseCaseTests
    {
        [Fact]
        public async Task ExecuteAsync_DeveCriarPedidoEAtualizarCache()
        {
            // Arrange
            var orderRepositoryMock = new Mock<IOrderRepository>();
            var orderCacheMock = new Mock<IOrderCache>();

            orderRepositoryMock.Setup(r => r.ExistsAsync("EX123")).ReturnsAsync(false);
            orderRepositoryMock.Setup(r => r.GetOrdersByStatusAsync(It.IsAny<OrderStatus>()))
                               .ReturnsAsync(new List<Order>());

            var useCase = new CreateOrderUseCase(orderRepositoryMock.Object, orderCacheMock.Object);

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
            orderRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once);
            orderCacheMock.Verify(c => c.SetOrderByExternalIdAsync(
                "EX123", It.Is<Order>(o => o.Total == 25.5m), It.IsAny<TimeSpan>()), Times.Once);

          
        }

        [Fact]
        public async Task ExecuteAsync_DeveIgnorarPedidoDuplicado()
        {
            // Arrange
            var orderRepositoryMock = new Mock<IOrderRepository>();
            var orderCacheMock = new Mock<IOrderCache>();

            orderRepositoryMock.Setup(x => x.ExistsAsync("EX456")).ReturnsAsync(true);

            var useCase = new CreateOrderUseCase(orderRepositoryMock.Object, orderCacheMock.Object);

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
            orderCacheMock.VerifyNoOtherCalls();
        }
    }
}
