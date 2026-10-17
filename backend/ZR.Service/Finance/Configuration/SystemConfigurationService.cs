using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Repository.Finance.Configuration;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Service.Finance.Configuration
{
    /// <summary>
    /// System Configuration Service - خدمة إعدادات النظام
    /// </summary>
    [AppService(ServiceType = typeof(ISystemConfigurationService), ServiceLifetime = LifeTime.Transient)]
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly ISystemConfigurationRepository _repository;

        public SystemConfigurationService(ISystemConfigurationRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get paginated list of configurations
        /// </summary>
        public PagedInfo<ConfigurationResponseDto> GetList(SystemConfigurationQueryDto query, PagerInfo pager)
        {
            var searchModel = new SystemConfiguration
            {
                Key = query.Key,
                ValueType = query.ValueType,
                CompanyID = query.CompanyId
            };

            var result = _repository.GetList(searchModel, pager);

            var dtoList = result.Result.Select(MapToDto).ToList();

            return new PagedInfo<ConfigurationResponseDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get configuration by ID
        /// </summary>
        public ConfigurationResponseDto GetById(Guid id)
        {
            var entity = _repository.GetById(id);
            return entity == null ? null : MapToDto(entity);
        }

        /// <summary>
        /// Get configuration by ID async
        /// </summary>
        public async Task<ConfigurationResponseDto> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        /// <summary>
        /// Get configuration by key
        /// </summary>
        public ConfigurationResponseDto GetByKey(string key, long? companyId = null)
        {
            var entity = _repository.GetByKey(key, companyId);
            return entity == null ? null : MapToDto(entity);
        }

        /// <summary>
        /// Get configuration by key async
        /// </summary>
        public async Task<ConfigurationResponseDto> GetByKeyAsync(string key, long? companyId = null)
        {
            var entity = await _repository.GetByKeyAsync(key, companyId);
            return entity == null ? null : MapToDto(entity);
        }

        /// <summary>
        /// Get all configurations
        /// </summary>
        public List<ConfigurationResponseDto> GetAll(long? companyId = null)
        {
            var entities = _repository.ListAll(companyId);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Get all configurations async
        /// </summary>
        public async Task<List<ConfigurationResponseDto>> GetAllAsync(long? companyId = null)
        {
            var entities = await _repository.ListAllAsync(companyId);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Create configuration
        /// </summary>
        public ConfigurationResponseDto Create(CreateConfigurationDto dto, string createdBy)
        {
            // Check if key already exists
            if (_repository.KeyExists(dto.Key, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_SYSTEM_KEY_ALREADY_EXISTS)
                    .WithParameter("Key", dto.Key);
            }

            Guid? createdByGuid = null;
            if (Guid.TryParse(createdBy, out Guid parsedGuid))
            {
                createdByGuid = parsedGuid;
            }

            var entity = new SystemConfiguration
            {
                Id = Guid.NewGuid(),
                Key = dto.Key,
                Value = dto.Value,
                ValueType = dto.ValueType,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                CompanyID = dto.CompanyId,
                Create_by = createdBy,
                Create_time = DateTime.Now
            };

            _repository.CreateConfiguration(entity);

            return MapToDto(entity);
        }

        /// <summary>
        /// Create configuration async
        /// </summary>
        public async Task<ConfigurationResponseDto> CreateAsync(CreateConfigurationDto dto, string createdBy)
        {
            // Check if key already exists
            if (await _repository.KeyExistsAsync(dto.Key, dto.CompanyId))
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_SYSTEM_KEY_ALREADY_EXISTS)
                    .WithParameter("Key", dto.Key);
            }

            Guid? createdByGuid = null;
            if (Guid.TryParse(createdBy, out Guid parsedGuid))
            {
                createdByGuid = parsedGuid;
            }

            var entity = new SystemConfiguration
            {
                Id = Guid.NewGuid(),
                Key = dto.Key,
                Value = dto.Value,
                ValueType = dto.ValueType,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                CompanyID = dto.CompanyId,
                Create_by = createdBy,
                Create_time = DateTime.Now
            };

            await _repository.CreateConfigurationAsync(entity);

            return MapToDto(entity);
        }

        /// <summary>
        /// Update configuration
        /// </summary>
        public int Update(UpdateConfigurationDto dto, string updatedBy)
        {
            // Check if key already exists for another configuration
            if (_repository.KeyExists(dto.Key, dto.CompanyId, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_SYSTEM_KEY_ALREADY_EXISTS)
                    .WithParameter("Key", dto.Key);
            }

            Guid? updatedByGuid = null;
            if (Guid.TryParse(updatedBy, out Guid parsedGuid))
            {
                updatedByGuid = parsedGuid;
            }

            var entity = new SystemConfiguration
            {
                Id = dto.Id,
                Key = dto.Key,
                Value = dto.Value,
                ValueType = dto.ValueType,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                Update_by = updatedBy,
                Update_time = DateTime.Now
            };

            return _repository.UpdateConfiguration(entity);
        }

        /// <summary>
        /// Update configuration async
        /// </summary>
        public async Task<int> UpdateAsync(UpdateConfigurationDto dto, string updatedBy)
        {
            // Check if key already exists for another configuration
            if (await _repository.KeyExistsAsync(dto.Key, dto.CompanyId, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_SYSTEM_KEY_ALREADY_EXISTS)
                    .WithParameter("Key", dto.Key);
            }

            Guid? updatedByGuid = null;
            if (Guid.TryParse(updatedBy, out Guid parsedGuid))
            {
                updatedByGuid = parsedGuid;
            }

            var entity = new SystemConfiguration
            {
                Id = dto.Id,
                Key = dto.Key,
                Value = dto.Value,
                ValueType = dto.ValueType,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                Update_by = updatedBy,
                Update_time = DateTime.Now
            };

            return await _repository.UpdateConfigurationAsync(entity);
        }

        /// <summary>
        /// Delete configuration
        /// </summary>
        public int Delete(Guid id)
        {
            return _repository.DeleteConfiguration(id);
        }

        /// <summary>
        /// Delete configuration async
        /// </summary>
        public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteConfigurationAsync(id);
        }

        /// <summary>
        /// Get configuration value by key
        /// </summary>
        public string GetValue(string key, long? companyId = null)
        {
            var entity = _repository.GetByKey(key, companyId);
            return entity?.Value;
        }

        /// <summary>
        /// Get configuration value by key async
        /// </summary>
        public async Task<string> GetValueAsync(string key, long? companyId = null)
        {
            var entity = await _repository.GetByKeyAsync(key, companyId);
            return entity?.Value;
        }

        /// <summary>
        /// Get typed configuration value
        /// </summary>
        public T GetValue<T>(string key, long? companyId = null) where T : IConvertible
        {
            var value = GetValue(key, companyId);
            if (string.IsNullOrEmpty(value))
                return default;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Get typed configuration value async
        /// </summary>
        public async Task<T> GetValueAsync<T>(string key, long? companyId = null) where T : IConvertible
        {
            var value = await GetValueAsync(key, companyId);
            if (string.IsNullOrEmpty(value))
                return default;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Map entity to DTO
        /// </summary>
        private ConfigurationResponseDto MapToDto(SystemConfiguration entity)
        {
            return new ConfigurationResponseDto
            {
                Id = entity.Id,
                Key = entity.Key,
                Value = entity.Value,
                ValueType = entity.ValueType,
                DescriptionAr = entity.DescriptionAr,
                DescriptionEn = entity.DescriptionEn,
                TenantId = null,
                CreatedBy = Guid.TryParse(entity.Create_by, out Guid createdByGuid) ? createdByGuid : (Guid?)null,
                CreatedAt = entity.Create_time,
                UpdatedBy = !string.IsNullOrEmpty(entity.Update_by) && Guid.TryParse(entity.Update_by, out Guid updatedByGuid) ? updatedByGuid : (Guid?)null,
                UpdatedAt = entity.Update_time
            };
        }
    }
}

