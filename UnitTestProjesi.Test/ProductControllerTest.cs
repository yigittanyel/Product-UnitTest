using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NuGet.ContentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestProjesi.Controllers;
using UnitTestProjesi.Models;
using UnitTestProjesi.Repository;

namespace UnitTestProjesi.Test
{
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsController _controller;
        private List<Product> _products;

        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsController(_mockRepo.Object);
            _products = new List<Product>()
            {
                new Product {Id=1,Name="Book",Price=100,Stock=10,Color="Red"},
                new Product {Id=2,Name="Pencil",Price=400,Stock=60,Color="Black"},
                new Product {Id=3,Name="Notebook",Price=600,Stock=30,Color="Blue"}
            };
        }

        #region INDEX TESTS


        [Fact]
        public async Task Index_CheckIsType_ReturnViewResult()
        {
            var result = await _controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ModelFill_ReturnViewWithModel()
        {
            _mockRepo.Setup(x => x.GetAll()).ReturnsAsync(_products);
            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>
                (viewResult.Model);

            Assert.Equal<int>(3, productList.Count());
        }
        #endregion

        #region DETAILS TESTS


        [Fact]
        public async Task Details_IdIsNull_ReturnRedirectToAction()
        {
            var result = await _controller.Details(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Details_IdInValid_ReturnNotFound()
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(0)).ReturnsAsync(product);

            var result = await _controller.Details(0);
            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        public async Task Details_CheckIsType_ReturnViewResult(int productId)
        {
            Product product = _products.FirstOrDefault(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Details(productId);
            var redirect = Assert.IsType<ViewResult>(result);
            var productResult = Assert.IsAssignableFrom<Product>(redirect.Model);

            Assert.Equal(product.Name, productResult.Name);

        }
        #endregion

        #region DELETE TESTS


        [Fact]
        public async Task Delete_IdIsNull_ReturnRedirectToAction()
        {
            var result = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ProductNull_ReturnRedirectToAction()
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(0)).ReturnsAsync(product);

            var result = await _controller.Delete(0);
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async Task Delete_CheckIsType_ReturnViewResult(int productId)
        {
            Product product = _products.FirstOrDefault(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Delete(productId);
            var redirect = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Product>(redirect.Model);

        }
        #endregion

        #region DELETE CONFIRMED TESTS


        [Theory]
        [InlineData(1)]
        public async Task DeleteConfirmed_ProductNull_ReturnRedirectToAction(int productId)
        {
            var result = await _controller.DeleteConfirmed(productId);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async Task DeleteConfirmed_CheckProductId_DeleteMethodExecute(int productId)
        {
            var product = _products.FirstOrDefault(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            await _controller.DeleteConfirmed(productId);
            _mockRepo.Verify(x => x.Delete(It.IsAny<Product>()), Times.Once);
        }
        #endregion

        #region CREATE TESTS

        [Fact]
        public void Create_CheckIsType_ReturViewResult()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_ModelStateInvalid_ReturViewWithModel()
        {
            _controller.ModelState.AddModelError("Name", "Name alanı gereklidir");

            var result = await _controller.Create(_products.First());
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async Task CreatePost_ModelStateValid_ReturnRedirect()
        {
            var result = await _controller.Create(_products.First());
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task CreatePost_IfModelStateValid_ExecuteCreateMethod()
        {
            Product newProduct = null;
            _mockRepo.Setup(x => x.Create(It.IsAny<Product>())).Callback<Product>
                (x => newProduct = x);

            var result = await _controller.Create(_products.First());

            _mockRepo.Verify(x => x.Create(It.IsAny<Product>()), Times.Once);
            Assert.Equal(_products.First().Id, newProduct.Id);
        }

        [Fact]
        public async Task CreatePost_IfModelStateInvalid_DontExecute()
        {
            _controller.ModelState.AddModelError("Name", "İsim alanı boş geçilemez");
            var result = await _controller.Create(_products.First());

            _mockRepo.Verify(x => x.Create(It.IsAny<Product>()), Times.Never);
        }
        #endregion

        #region EDIT TESTS

        [Fact]
        public async void Edit_CheckIdNull_ReturnNotFound()
        {
            var result = await _controller.Edit(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async Task Edit_IdInvalid_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(3)]
        public async Task Edit_ActionExecute_ReturnProduct(int productId)
        {
            var product = _products.FirstOrDefault(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Edit(productId);
            var redirect = Assert.IsType<ViewResult>(result);

            var productResult = Assert.IsAssignableFrom<Product>(redirect.Model);

            Assert.Equal(product.Id, productResult.Id);
            Assert.Equal(product.Name, productResult.Name);
        }

        [Theory]
        [InlineData(1)]
        public async Task EditPost_IdIsNotEqualProductId_ReturnNotFound(int productId)
        {
            var result = await _controller.Edit(2, _products.First(x => x.Id == productId));
            var redirect=Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404,redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        public async Task EditPost_InvalidModelState_ReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name", "");
            var result=await _controller.Edit(productId, _products.First(x => x.Id == productId));

            var redirect = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(redirect.Model);
        }
        [Theory]
        [InlineData(1)]
        public async Task EditPost_ValidModelState_ReturnRedirect(int productId)
        {
            var result = await _controller.Edit(productId, _products.First(x => x.Id == productId));

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(2)]
        public async Task EditPost_ValidModelState_ExecuteEditMethod(int productId)
        {
            var product=_products.First(x=>x.Id == productId);
            _mockRepo.Setup(x => x.Update(product));

            await _controller.Edit(productId, product);
            _mockRepo.Verify(x=>x.Update(It.IsAny<Product>()),Times.Once);
        }
        #endregion

    }
}
