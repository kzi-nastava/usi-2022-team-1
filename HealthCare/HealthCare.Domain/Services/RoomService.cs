using HealthCare.Data.Entities;
using HealthCare.Domain.DTOs;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using HealthCare.Repositories;

namespace HealthCare.Domain.Services;

public class RoomService : IRoomService
{
    private IRoomRepository _roomRepository;
    private IRoomTypeRepository _roomTypeRepository;

    public RoomService(IRoomRepository roomRepository, IRoomTypeRepository roomTypeRepository) 
    {
        _roomRepository = roomRepository;
        _roomTypeRepository = roomTypeRepository;
    }

    public async Task<IEnumerable<RoomDomainModel>> ReadAll()
    {
        IEnumerable<RoomDomainModel> rooms = await GetAll();
        List<RoomDomainModel> result = new List<RoomDomainModel>();
        foreach (RoomDomainModel item in rooms)
        {
            if (!item.IsDeleted) result.Add(item);
        }
        return result;
    }

  public async Task<IEnumerable<RoomDomainModel>> GetAll()
    {
        IEnumerable<Room> rooms = await _roomRepository.GetAll();
        if (rooms == null)
            return new List<RoomDomainModel>();
            
        List<RoomDomainModel> results = new List<RoomDomainModel>();
        RoomDomainModel roomModel;
        foreach (Room item in rooms)
        {
            roomModel = new RoomDomainModel
            {
                IsDeleted = item.IsDeleted,
                Id = item.Id,
                RoomName = item.RoomName,
                RoomTypeId = item.RoomTypeId,
            };
            if(item.RoomType != null) 
            {
                roomModel.RoomType = new RoomTypeDomainModel 
                {
                    IsDeleted = item.RoomType.IsDeleted,
                    Id = item.RoomType.Id,
                    RoleName = item.RoomType.RoleName,
                    Purpose = item.RoomType.Purpose,
                };
            }
            roomModel.Inventories = new List<InventoryDomainModel>();
            roomModel.Operations = new List<OperationDomainModel>();
            if (item.Inventories != null) 
            {
                foreach (Inventory inventory in item.Inventories) 
                {
                    InventoryDomainModel inventoryModel = new InventoryDomainModel 
                    {
                        IsDeleted = inventory.IsDeleted,
                        RoomId = inventory.RoomId,
                        Amount = inventory.Amount,
                        EquipmentId = inventory.EquipmentId,
                    };
                    inventoryModel.Equipment = new EquipmentDomainModel 
                    {
                        Id = inventory.Equipment.Id,
                        EquipmentTypeId = inventory.Equipment.EquipmentTypeId,
                        IsDeleted = inventory.Equipment.IsDeleted,
                        Name = inventory.Equipment.Name,
                    };
                    if (inventory.Equipment.EquipmentType != null)
                    {
                        inventoryModel.Equipment.EquipmentType = new EquipmentTypeDomainModel
                        {
                            Id = inventory.Equipment.EquipmentType.Id,
                            Name = inventory.Equipment.EquipmentType.Name,
                            IsDeleted = inventory.Equipment.EquipmentType.IsDeleted
                        };
                    }

                    roomModel.Inventories.Add(inventoryModel);
                }
            }
            if (item.Operations != null) 
            {
                foreach (Operation operation in item.Operations) 
                {
                    OperationDomainModel operationModel = new OperationDomainModel 
                    {
                        DoctorId = operation.DoctorId,
                        RoomId = operation.DoctorId,
                        PatientId = operation.DoctorId,
                        Duration = operation.Duration,
                        IsDeleted = operation.IsDeleted
                    };
                    roomModel.Operations.Add(operationModel);
                }
            }
            results.Add(roomModel);
        }
        return results;
    }

    public async Task<RoomDomainModel> Create(CURoomDTO dto)
    {
        Room newRoom = new Room();
        newRoom.IsDeleted = false;
        newRoom.RoomName = dto.RoomName;
        RoomType roomType = await _roomTypeRepository.GetById(dto.RoomTypeId);
        if (roomType == null)
            throw new RoomTypeNotFoundException();
        newRoom.RoomType = roomType;
        newRoom.RoomTypeId = roomType.Id;
        _ = _roomRepository.Post(newRoom);
        _roomRepository.Save();

        return ParseToModel(newRoom);
    }

    public async Task<RoomDomainModel> Update(CURoomDTO dto)
    {
        Room room = await _roomRepository.GetRoomById(dto.RoomId);
        room.RoomName = dto.RoomName;
        RoomType roomType = await _roomTypeRepository.GetById(dto.RoomTypeId);
        if (roomType == null)
            throw new RoomTypeNotFoundException();
        room.RoomType = roomType;
        room.RoomTypeId = roomType.Id;
        _ = _roomRepository.Update(room);
        _roomRepository.Save();

        return ParseToModel(room);
    }

    public async Task<RoomDomainModel> Delete(decimal id)
    {
        Room deletedRoom = await _roomRepository.GetRoomById(id);
        deletedRoom.IsDeleted = true;
        _ = _roomRepository.Update(deletedRoom);
        _roomRepository.Save();
        return ParseToModel(deletedRoom);
    }

    public static RoomDomainModel ParseToModel(Room room)
    {
        RoomDomainModel roomModel = new RoomDomainModel 
        {
            Id = room.Id,
            RoomName = room.RoomName,
            RoomTypeId = room.RoomTypeId,
            IsDeleted = room.IsDeleted
        };
        
        return roomModel;
    }
    
    public static Room ParseFromModel(RoomDomainModel roomModel)
    {
        Room room = new Room 
        {
            Id = roomModel.Id,
            RoomName = roomModel.RoomName,
            RoomTypeId = roomModel.RoomTypeId,
            IsDeleted = roomModel.IsDeleted
        };
        
        return room;
    }
}