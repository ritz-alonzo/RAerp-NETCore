using AutoMapper;
using RA.Inventory.Domain;
using RA.Inventory.DTO.InventoryReservation;
using RA.Inventory.DTO.InventoryStock;
using RA.Inventory.DTO.InventoryTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Mapping
{
    public class InventoryMappingProfile : Profile
    {
        public InventoryMappingProfile()
        {
            #region Inventory Stock DTOs
            CreateMap<InventoryStock, InventoryStockResponseDto>();
            CreateMap<InventoryStockRequestDto, InventoryStock>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Inventory Transaction DTOs
            CreateMap<InventoryTransaction, InventoryTransactionResponseDto>();
            CreateMap<InventoryTransactionRequestDto, InventoryTransaction>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Inventory Reservation DTOs
            CreateMap<InventoryReservation, InventoryReservationResponseDto>();
            CreateMap<InventoryReservationRequestDto, InventoryReservation>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
        }
    }
}
