using Moq;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace OrderService.Tests.UseCases
{
    public class GetOrdersByStatusUseCaseTests
    {
        [Fact]
        public async Task ExecuteAsync_DeveRetornarPedidosDoCache_QuandoDisponivel()
        {
            // Arrange
            var pedidos = new List<Order>
        {
            new("EX01") { },
            new("EX02") { }
        };

            var cacheMock = new Mock<IOrderCache>();
            cacheMock.Setup(c => c.GetOrdersByStatusAsync(OrderStatus.Processed))
                     .ReturnsAsync(pedidos);

            var repoMock = new Mock<IOrderRepository>();

            var useCase = new GetOrdersByStatusUseCase(repoMock.Object, cacheMock.Object);

            // Act
            var result = await useCase.ExecuteAsync(OrderStatus.Processed);

            // Assert
            result.Should().HaveCount(2);
            repoMock.Verify(r => r.GetOrdersByStatusAsync(It.IsAny<OrderStatus>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_DeveBuscarNoBancoQuandoCacheForNulo_ESalvarNoCache()
        {
            // Arrange
            var pedidos = new List<Order>
        {
            new("EX01") { },
            new("EX02") { }
        };

            var cacheMock = new Mock<IOrderCache>();
            cacheMock.Setup(c => c.GetOrdersByStatusAsync(OrderStatus.Processed))
                     .ReturnsAsync((IEnumerable<Order>?)null);

            var repoMock = new Mock<IOrderRepository>();
            repoMock.Setup(r => r.GetOrdersByStatusAsync(OrderStatus.Processed))
                    .ReturnsAsync(pedidos);

            var useCase = new GetOrdersByStatusUseCase(repoMock.Object, cacheMock.Object);

            // Act
            var result = await useCase.ExecuteAsync(OrderStatus.Processed);

            // Assert
            result.Should().HaveCount(2);
            repoMock.Verify(r => r.GetOrdersByStatusAsync(OrderStatus.Processed), Times.Once);
            cacheMock.Verify(c => c.SetOrdersByStatusAsync(OrderStatus.Processed, pedidos, It.IsAny<TimeSpan>()), Times.Once);
        }
    }
}
