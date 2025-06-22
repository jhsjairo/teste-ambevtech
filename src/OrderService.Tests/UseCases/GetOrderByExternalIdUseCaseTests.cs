using Moq;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace OrderService.Tests.UseCases
{
    public class GetOrderByExternalIdUseCaseTests
    {
        [Fact]
        public async Task ExecuteAsync_DeveRetornarPedidoDoCache_QuandoExistir()
        {
            // Arrange
            var externalOrderId = "EX789";
            var pedidoMock = new Order(externalOrderId);
            pedidoMock.AddItem("Produto Z", 1, 50.0m);

            var cacheMock = new Mock<IOrderCache>();
            cacheMock.Setup(c => c.GetOrderByExternalIdAsync(externalOrderId))
                     .ReturnsAsync(pedidoMock);

            var repoMock = new Mock<IOrderRepository>();

            var useCase = new GetOrderByExternalIdUseCase(repoMock.Object, cacheMock.Object);

            // Act
            var result = await useCase.ExecuteAsync(externalOrderId);

            // Assert
            result.Should().NotBeNull();
            result!.ExternalOrderId.Should().Be("EX789");
            repoMock.Verify(r => r.GetByExternalOrderIdAsync(It.IsAny<string>()), Times.Never); // não deve acessar o banco
        }

        [Fact]
        public async Task ExecuteAsync_DeveRetornarPedidoDoBanco_QuandoNaoEstiverEmCache()
        {
            // Arrange
            var externalOrderId = "EX123";
            var pedidoMock = new Order(externalOrderId);
            pedidoMock.AddItem("Produto X", 2, 10);

            var cacheMock = new Mock<IOrderCache>();
            cacheMock.Setup(c => c.GetOrderByExternalIdAsync(externalOrderId))
                     .ReturnsAsync((Order?)null);

            var repoMock = new Mock<IOrderRepository>();
            repoMock.Setup(r => r.GetByExternalOrderIdAsync(externalOrderId))
                    .ReturnsAsync(pedidoMock);

            var useCase = new GetOrderByExternalIdUseCase(repoMock.Object, cacheMock.Object);

            // Act
            var result = await useCase.ExecuteAsync(externalOrderId);

            // Assert
            result.Should().NotBeNull();
            result!.ExternalOrderId.Should().Be("EX123");

            cacheMock.Verify(c => c.SetOrderByExternalIdAsync(externalOrderId, pedidoMock, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_DeveRetornarNull_QuandoNaoEncontradoNoCacheENoBanco()
        {
            // Arrange
            var externalOrderId = "NOTFOUND";

            var cacheMock = new Mock<IOrderCache>();
            cacheMock.Setup(c => c.GetOrderByExternalIdAsync(externalOrderId))
                     .ReturnsAsync((Order?)null);

            var repoMock = new Mock<IOrderRepository>();
            repoMock.Setup(r => r.GetByExternalOrderIdAsync(externalOrderId))
                    .ReturnsAsync((Order?)null);

            var useCase = new GetOrderByExternalIdUseCase(repoMock.Object, cacheMock.Object);

            // Act
            var result = await useCase.ExecuteAsync(externalOrderId);

            // Assert
            result.Should().BeNull();
            cacheMock.Verify(c => c.SetOrderByExternalIdAsync(It.IsAny<string>(), It.IsAny<Order>(), It.IsAny<TimeSpan>()), Times.Never);
        }
    }


}
