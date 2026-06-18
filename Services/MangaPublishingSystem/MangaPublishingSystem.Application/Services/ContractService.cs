using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Contracts;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.Services
{
    public class ContractService : GenericService<Contract>, IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISeriesRepository _seriesRepository;

        public ContractService(
            IContractRepository repository,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            ISeriesRepository seriesRepository) : base(repository, unitOfWork)
        {
            _contractRepository = repository;
            _userRepository = userRepository;
            _seriesRepository = seriesRepository;
        }

        public async Task<ContractDto> CreateContractAsync(CreateContractDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null || user.RoleId != 4) // Phải là Mangaka
            {
                throw new NotFoundException("Không tìm thấy tác giả Mangaka.");
            }

            var series = await _seriesRepository.GetByIdAsync(dto.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.MangakaId != dto.UserId)
            {
                throw new BadRequestException("Bộ truyện không thuộc về tác giả này.");
            }

            if (dto.BaseGenkouryoPrice < 0)
            {
                throw new BadRequestException("Đơn giá nhuận bút trang không được nhỏ hơn 0.");
            }

            // Kiểm tra xem đã có hợp đồng Active cho bộ truyện này chưa
            var existingActive = await _contractRepository.FindAsync(c => c.SeriesId == dto.SeriesId && c.Status == "Active");
            if (existingActive.Any())
            {
                throw new ConflictException("Bộ truyện này đã có một hợp đồng đang hoạt động.");
            }

            var contract = new Contract
            {
                UserId = dto.UserId,
                SeriesId = dto.SeriesId,
                BaseGenkouryoPrice = dto.BaseGenkouryoPrice,
                SignedDate = DateTime.Now,
                Status = "Active"
            };

            await _contractRepository.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            var created = await _contractRepository.GetContractWithDetailsAsync(contract.Id);
            return MapToDto(created!);
        }

        public async Task<ContractDto> UpdateContractAsync(int id, UpdateContractDto dto)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                throw new NotFoundException("Hợp đồng không tồn tại.");
            }

            if (dto.BaseGenkouryoPrice < 0)
            {
                throw new BadRequestException("Đơn giá nhuận bút trang không được nhỏ hơn 0.");
            }

            if (dto.Status == "Active" && contract.Status != "Active")
            {
                var existingActive = await _contractRepository.FindAsync(c => c.SeriesId == contract.SeriesId && c.Status == "Active" && c.Id != contract.Id);
                if (existingActive.Any())
                {
                    throw new ConflictException("Bộ truyện này đã có một hợp đồng đang hoạt động.");
                }
            }

            contract.BaseGenkouryoPrice = dto.BaseGenkouryoPrice;
            contract.Status = dto.Status;
            
            _contractRepository.Update(contract);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _contractRepository.GetContractWithDetailsAsync(contract.Id);
            return MapToDto(updated!);
        }

        public async Task<ContractDto> GetContractByIdAsync(int id)
        {
            var contract = await _contractRepository.GetContractWithDetailsAsync(id);
            if (contract == null)
            {
                throw new NotFoundException("Hợp đồng không tồn tại.");
            }
            return MapToDto(contract);
        }

        public async Task<IEnumerable<ContractDto>> GetContractsAsync()
        {
            var contracts = await _contractRepository.GetContractsWithDetailsAsync();
            return contracts.Select(MapToDto);
        }

        public async System.Threading.Tasks.Task DeleteContractAsync(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                throw new NotFoundException("Hợp đồng không tồn tại.");
            }
            _contractRepository.Delete(contract);
            await _unitOfWork.SaveChangesAsync();
        }

        private static ContractDto MapToDto(Contract contract)
        {
            return new ContractDto
            {
                Id = contract.Id,
                UserId = contract.UserId,
                MangakaName = contract.User?.FullName,
                SeriesId = contract.SeriesId,
                SeriesTitle = contract.Series?.Title,
                BaseGenkouryoPrice = contract.BaseGenkouryoPrice,
                SignedDate = contract.SignedDate,
                Status = contract.Status,
                CreateAt = contract.CreateAt
            };
        }
    }
}