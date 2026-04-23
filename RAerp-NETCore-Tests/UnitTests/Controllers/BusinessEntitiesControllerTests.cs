using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using RA.BusinessEntities.Controllers;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.Factories;
using RA.BusinessEntities.Helpers;
using RA.BusinessEntities.Services;
using RA.Core.Models.OverviewModels;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.Categories;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Data.Domain.Addresses;
using RA.Data.Domain.EntityTypes;
using RA.Data.Domain.Users;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RAerp.Helpers.AddressHelper;
using RAerp.Helpers.UserHelper;
using RAerp.Services.AddressServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RAerp_NETCore_Tests.UnitTests.Controllers
{
    [TestFixture]
    public class BusinessEntitiesControllerTests
    {
        #region Constants
        private Mock<IBusinessEntityService> _mockBusinessEntityService;
        private Mock<IEntityTypeManager> _mockEntityTypeManager;
        private Mock<IBusinessEntityModelFactory> _mockBusinessEntityModelFactory;
        private Mock<IMapper> _mockMapper;
        private Mock<IUserIdentity> _mockUserIdentity;
        private Mock<IAddressService> _mockAddressService;
        private BusinessEntitiesController _controller;
        private Guid _testEntityTypeId;
        private Guid _testEntityId;
        private Guid _testUserId;
        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            _mockBusinessEntityService = new Mock<IBusinessEntityService>();
            _mockEntityTypeManager = new Mock<IEntityTypeManager>();
            _mockBusinessEntityModelFactory = new Mock<IBusinessEntityModelFactory>();
            _mockMapper = new Mock<IMapper>();
            _mockUserIdentity = new Mock<IUserIdentity>();
            _mockAddressService = new Mock<IAddressService>();

            _controller = new BusinessEntitiesController(
                _mockBusinessEntityService.Object,
                _mockEntityTypeManager.Object,
                _mockBusinessEntityModelFactory.Object,
                _mockMapper.Object,
                _mockUserIdentity.Object,
                _mockAddressService.Object);

            _testEntityTypeId = Guid.NewGuid();
            _testEntityId = Guid.NewGuid();
            _testUserId = Guid.NewGuid();

            // Setup default HttpContext
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        #endregion

        #region Configuration Tests

        [Test]
        public async Task Configuration_Get_WithNullEntityTypeId_ReturnsJsonError()
        {
            // Arrange
            Guid nullEntityTypeId = Guid.Empty;

            // Act
            var result = await _controller.Configuration(nullEntityTypeId, "test");

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);
        }

        [Test]
        public async Task Configuration_Get_WithNullEntityTypeId_ReturnsJsonErrorMessage_EntityTypeNotExists()
        {
            // Arrange
            Guid nullEntityTypeId = Guid.Empty;

            // Act
            JsonResult result = (JsonResult)await _controller.Configuration(nullEntityTypeId, "test");

            var errorMsg = result.Value?.GetType().GetProperty("Error")?.GetValue(result.Value)?.ToString();

            // Assert
            Assert.That(errorMsg, Is.EqualTo(EntityTypeMessages.EntityTypeNotExists));
        }

        [Test]
        public async Task Configuration_Get_WithValidEntityTypeId_ReturnsViewWithModel()
        {
            // Arrange
            var configureModel = new BusinessEntityConfigureModel { EntityTypeId = _testEntityTypeId };
            _mockBusinessEntityModelFactory
                .Setup(x => x.PrepareBusinessEntityConfigureModelAsync(_testEntityTypeId, "test"))
                .ReturnsAsync(configureModel);

            // Act
            var result = await _controller.Configuration(_testEntityTypeId, "test");

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            _mockBusinessEntityModelFactory.Verify(
                x => x.PrepareBusinessEntityConfigureModelAsync(_testEntityTypeId, "test"),
                Times.Once);
        }

        [Test]
        public async Task Configuration_Post_WithNullModel_ReturnsJsonError()
        {
            // Arrange
            BusinessEntityConfigureModel nullModel = null;

            // Act
            var result = await _controller.Configuration(nullModel);

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);
        }

        [Test]
        public async Task Configuration_Post_WithNullEntityTypeId_ReturnsJsonError()
        {
            // Arrange
            var model = new BusinessEntityConfigureModel { EntityTypeId = Guid.Empty };

            // Act
            var result = await _controller.Configuration(model);

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);
        }

        [Test]
        public async Task Configuration_Post_WithValidModel_UpdatesSettings()
        {
            // Arrange
            var model = new BusinessEntityConfigureModel
            {
                EntityTypeId = _testEntityTypeId,
                SystemName = "test"
            };

            var settings = new BusinessEntitySetting();
            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, "test"))
                .ReturnsAsync(settings);

            _mockMapper.Setup(x => x.Map(model, settings)).Returns(settings);

            // Act
            var result = await _controller.Configuration(model);

            // Assert
            Assert.IsNotNull(result);
            _mockEntityTypeManager.Verify(
                x => x.UpdateSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(
                    It.IsAny<BusinessEntitySetting>(), _testEntityTypeId),
                Times.Once);
        }

        [Test]
        public async Task Configuration_Post_WhenSettingsNull_InsertsNewSetting()
        {
            // Arrange
            var model = new BusinessEntityConfigureModel
            {
                EntityTypeId = _testEntityTypeId,
                SystemName = "test"
            };

            _mockEntityTypeManager
                .SetupSequence(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, "test"))
                .ReturnsAsync((BusinessEntitySetting)null)
                .ReturnsAsync(new BusinessEntitySetting());

            _mockEntityTypeManager
                .Setup(x => x.InsertEntitySettingAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, "test"))
                .Returns(Task.CompletedTask);

            _mockMapper.Setup(x => x.Map(It.IsAny<BusinessEntityConfigureModel>(), It.IsAny<BusinessEntitySetting>()))
                .Returns(new BusinessEntitySetting());

            // Act
            var result = await _controller.Configuration(model);

            // Assert
            Assert.IsNotNull(result);
            _mockEntityTypeManager.Verify(
                x => x.InsertEntitySettingAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, "test"),
                Times.Once);
        }

        #endregion

        #region List Tests

        [Test]
        public async Task List_WithNullEntityTypeId_ReturnsNotFound()
        {
            // Arrange
            Guid nullEntityTypeId = Guid.Empty;

            // Act
            var result = await _controller.List(nullEntityTypeId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task List_WhenEntityTypeSettingNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync((BusinessEntitySetting)null);

            // Act
            var result = await _controller.List(_testEntityTypeId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task List_WhenEntityTypeSettingDisabled_ReturnsNotFound()
        {
            // Arrange
            var setting = new BusinessEntitySetting { Enabled = false };
            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            // Act
            var result = await _controller.List(_testEntityTypeId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task List_WithValidEntityTypeIdAndEnabledSetting_ReturnsView()
        {
            // Arrange
            var setting = new BusinessEntitySetting { Enabled = true };
            var model = new BusinessEntitySearchModel();

            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            _mockBusinessEntityModelFactory
                .Setup(x => x.PrepareBusinessEntitySearchModelAsync(It.IsAny<BusinessEntitySearchModel>(), 10, 1))
                .ReturnsAsync(model);

            // Act
            var result = await _controller.List(_testEntityTypeId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        #endregion

        #region BusinessEntityListSearch Tests

        [Test]
        public async Task BusinessEntityListSearch_WithValidSearchModel_ReturnsPartialView()
        {
            // Arrange
            var searchModel = new BusinessEntitySearchModel();
            var listModel = new BusinessEntityListModel();

            _mockBusinessEntityModelFactory
                .Setup(x => x.PrepareBusinessEntityListModelAsync(searchModel))
                .ReturnsAsync(listModel);

            // Act
            var result = await _controller.BusinessEntityListSearch(searchModel);

            // Assert
            Assert.IsInstanceOf<PartialViewResult>(result);
            _mockBusinessEntityModelFactory.Verify(
                x => x.PrepareBusinessEntityListModelAsync(searchModel),
                Times.Once);
        }

        #endregion

        #region Index Tests

        [Test]
        public async Task Index_WithNullId_ReturnsNotFound()
        {
            // Arrange
            Guid nullId = Guid.Empty;

            // Act
            var result = await _controller.Index(nullId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Index_WhenEntityNotFound_ReturnsNotFound()
        {
            var entity = new BusinessEntity();
            // Arrange
            _mockBusinessEntityService
                .Setup(x => x.GetByIdAsync(_testEntityId))
                .ReturnsAsync(entity);

            // Act
            var result = await _controller.Index(_testEntityId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Index_WhenEntityTypeIdNull_ReturnsNotFound()
        {
            // Arrange
            var entity = new BusinessEntity { Id = _testEntityId, EntityTypeId = Guid.Empty };
            _mockBusinessEntityService
                .Setup(x => x.GetByIdAsync(_testEntityId))
                .ReturnsAsync(entity);

            // Act
            var result = await _controller.Index(_testEntityId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Index_WithValidEntity_ReturnsViewWithModel()
        {
            // Arrange
            var entity = new BusinessEntity { Id = _testEntityId, EntityTypeId = _testEntityTypeId };
            var model = new BusinessEntityModel();

            _mockBusinessEntityService
                .Setup(x => x.GetByIdAsync(_testEntityId))
                .ReturnsAsync(entity);

            _mockBusinessEntityModelFactory
                .Setup(x => x.PrepareBusinessEntityModelAsync(It.IsAny<BusinessEntityModel>(), entity, _testEntityTypeId))
                .ReturnsAsync(model);

            // Act
            var result = await _controller.Index(_testEntityId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            _mockBusinessEntityService.Verify(x => x.GetByIdAsync(_testEntityId), Times.Once);
        }

        #endregion

        #region Create GET Tests

        [Test]
        public async Task Create_Get_WithNullEntityTypeId_ReturnsNotFound()
        {
            // Arrange
            Guid nullEntityTypeId = Guid.Empty;

            // Act
            var result = await _controller.Create(nullEntityTypeId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Create_Get_WhenEntityTypeSettingNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync((BusinessEntitySetting)null);

            // Act
            var result = await _controller.Create(_testEntityTypeId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Create_Get_WhenEntityTypeSettingDisabled_ReturnsNotFound()
        {
            // Arrange
            var setting = new BusinessEntitySetting { Enabled = false };
            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            // Act
            var result = await _controller.Create(_testEntityTypeId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Create_Get_WithValidEntityTypeId_ReturnsView()
        {
            // Arrange
            var setting = new BusinessEntitySetting { Enabled = true };
            var model = new BusinessEntityModel();

            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            _mockBusinessEntityModelFactory
                .Setup(x => x.PrepareBusinessEntityModelAsync(It.IsAny<BusinessEntityModel>(), null, _testEntityTypeId))
                .ReturnsAsync(model);

            // Act
            var result = await _controller.Create(_testEntityTypeId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        #endregion

        #region Create POST Tests

        [Test]
        public async Task Create_Post_WhenEntityTypeSettingNotFound_ReturnsNotFound()
        {
            // Arrange
            var model = new BusinessEntityModel { EntityTypeId = _testEntityTypeId };
            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync((BusinessEntitySetting)null);

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Create_Post_WithInvalidModelState_ReturnsView()
        {
            // Arrange
            var model = new BusinessEntityModel { EntityTypeId = _testEntityTypeId };
            var setting = new BusinessEntitySetting { Enabled = true };

            _controller.ModelState.AddModelError("Name", "Name is required");

            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task Create_Post_WithValidModel_InsertsEntityAndRedirects()
        {
            // Arrange
            var model = new BusinessEntityModel
            {
                EntityTypeId = _testEntityTypeId,
                Name = "Test Entity"
            };
            var setting = new BusinessEntitySetting { Enabled = true, AddressEnabled = false };
            var entity = new BusinessEntity { Id = _testEntityId, EntityTypeId = _testEntityTypeId };

            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            _mockMapper.Setup(x => x.Map<BusinessEntity>(model)).Returns(entity);
            _mockUserIdentity
                .Setup(x => x.GetCurrentUserAsync(It.IsAny<HttpContext>()))
                .ReturnsAsync(new User { Id = _testUserId });

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            _mockBusinessEntityService.Verify(x => x.InsertAsync(It.IsAny<BusinessEntity>()), Times.Once);
        }

        [Test]
        public async Task Create_Post_WithAddressEnabled_CreatesAddressAndEntity()
        {
            // Arrange
            var addressModel = new AddressOverviewModel { RegionCode = "US" };
            var model = new BusinessEntityModel
            {
                EntityTypeId = _testEntityTypeId,
                Name = "Test Entity",
                Address = addressModel
            };
            var setting = new BusinessEntitySetting { Enabled = true, AddressEnabled = true };
            var entity = new BusinessEntity { Id = _testEntityId, EntityTypeId = _testEntityTypeId };
            var address = new Address { Id = Guid.NewGuid() };

            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            _mockMapper.Setup(x => x.Map<BusinessEntity>(model)).Returns(entity);
            _mockUserIdentity
                .Setup(x => x.GetCurrentUserAsync(It.IsAny<HttpContext>()))
                .ReturnsAsync(new User { Id = _testUserId });
            _mockAddressService
                .Setup(x => x.Insert(It.IsAny<Address>()))
                .ReturnsAsync(address);

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
        }

        #endregion

        #region Edit Tests

        [Test]
        public async Task Edit_WhenEntityTypeSettingNotFound_ReturnsNotFound()
        {
            // Arrange
            var model = new BusinessEntityModel { EntityTypeId = _testEntityTypeId };
            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync((BusinessEntitySetting)null);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Edit_WithInvalidModelState_ReturnsView()
        {
            // Arrange
            var model = new BusinessEntityModel { EntityTypeId = _testEntityTypeId, Id = _testEntityId };
            var setting = new BusinessEntitySetting { Enabled = true };

            _controller.ModelState.AddModelError("Name", "Name is required");

            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task Edit_WithValidModel_UpdatesEntityAndRedirects()
        {
            // Arrange
            var model = new BusinessEntityModel
            {
                Id = _testEntityId,
                EntityTypeId = _testEntityTypeId,
                Name = "Updated Entity"
            };
            var setting = new BusinessEntitySetting { Enabled = true, AddressEnabled = false };
            var entity = new BusinessEntity { Id = _testEntityId, EntityTypeId = _testEntityTypeId };

            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            _mockMapper.Setup(x => x.Map<BusinessEntity>(model)).Returns(entity);
            _mockUserIdentity
                .Setup(x => x.GetCurrentUserAsync(It.IsAny<HttpContext>()))
                .ReturnsAsync(new User { Id = _testUserId });

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            _mockBusinessEntityService.Verify(x => x.UpdateAsync(It.IsAny<BusinessEntity>()), Times.Once);
        }

        [Test]
        public async Task Edit_WithAddressUpdate_UpdatesAddressAndEntity()
        {
            // Arrange
            var addressModel = new AddressOverviewModel { RegionCode = "US" };
            var model = new BusinessEntityModel
            {
                Id = _testEntityId,
                EntityTypeId = _testEntityTypeId,
                Address = addressModel
            };
            var setting = new BusinessEntitySetting { Enabled = true, AddressEnabled = true };
            var entity = new BusinessEntity { Id = _testEntityId, EntityTypeId = _testEntityTypeId };
            var existingAddress = new Address { Id = Guid.NewGuid() };

            _mockEntityTypeManager
                .Setup(x => x.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(_testEntityTypeId, null))
                .ReturnsAsync(setting);

            _mockMapper.Setup(x => x.Map<BusinessEntity>(model)).Returns(entity);
            _mockUserIdentity
                .Setup(x => x.GetCurrentUserAsync(It.IsAny<HttpContext>()))
                .ReturnsAsync(new User { Id = _testUserId });
            _mockAddressService
                .Setup(x => x.Insert(It.IsAny<Address>()))
                .ReturnsAsync(existingAddress);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task Delete_WithNullId_ReturnsNotFound()
        {
            // Arrange
            Guid nullId = Guid.Empty;

            // Act
            var result = await _controller.Delete(nullId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Delete_WhenEntityNotFound_ReturnsNotFound()
        {
            // Arrange
            var entity = new BusinessEntity();
            _mockBusinessEntityService
                .Setup(x => x.GetByIdAsync(_testEntityId))
                .ReturnsAsync(entity);

            // Act
            var result = await _controller.Delete(_testEntityId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Delete_WhenEntityTypeIdNull_ReturnsNotFound()
        {
            // Arrange
            var entity = new BusinessEntity { Id = _testEntityId, EntityTypeId = Guid.Empty };
            _mockBusinessEntityService
                .Setup(x => x.GetByIdAsync(_testEntityId))
                .ReturnsAsync(entity);

            // Act
            var result = await _controller.Delete(_testEntityId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Delete_WithValidEntity_DeletesEntityAndRedirects()
        {
            // Arrange
            var entity = new BusinessEntity { Id = _testEntityId, EntityTypeId = _testEntityTypeId };
            _mockBusinessEntityService
                .Setup(x => x.GetByIdAsync(_testEntityId))
                .ReturnsAsync(entity);

            _mockBusinessEntityService
                .Setup(x => x.DeleteAsync(entity))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(_testEntityId);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("List"));
            _mockBusinessEntityService.Verify(x => x.DeleteAsync(entity), Times.Once);
        }

        #endregion
    }
}