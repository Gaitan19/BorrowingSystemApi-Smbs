﻿using AutoMapper;
using BorrowingSystemAPI.DTOs;
using BorrowingSystemAPI.DTOs.RequestDTOs;
using BorrowingSystemAPI.Exceptions;
using BorrowingSystemAPI.Interfaces.Repository;
using BorrowingSystemAPI.Models;
using BorrowingSystemAPI.Repositories;
using RequestItemDTO = BorrowingSystemAPI.DTOs.RequestDTOs.RequestItemDTO;

namespace BorrowingSystemAPI.Services
{
    public class RequestService
    {

        private readonly IRequestRepository _requestRepository;
        private readonly IRequestItemRepository _requestItemRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IMovementRepository _movementRepository;
        private readonly IMovementTypeRepository _movementTypeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public RequestService(IUserRepository userRepository, IRequestRepository requestRepository, IRequestItemRepository requestItemRepository, IItemRepository itemRepository, IMovementRepository movementRepository, IMovementTypeRepository movementTypeRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _requestRepository = requestRepository;
            _requestItemRepository = requestItemRepository;
            _itemRepository = itemRepository;
            _movementRepository = movementRepository;
            _movementTypeRepository = movementTypeRepository;
            _mapper = mapper;
        }

        public IEnumerable<Request> GetAllRequests()
        {
            var requests = _requestRepository.GetAllRequests();
            return requests;
        }

        public Request? GetRequestById(Guid id)
        {
            return _requestRepository.GetRequestById(id);
        }


        public RequestDTO UpdateRequest(Guid id, UpdateRequestDTO dto)
        {
            var existingRequest = _requestRepository.GetRequestById(id, false);


            if (existingRequest == null)
                throw new ServiceException("Request not found.", ErrorCode.NotFound);

            if (existingRequest.RequestStatus != RequestStatus.Pending || existingRequest.ReturnIsCompleted)
                throw new ServiceException("You cannot edit an approved or rejected request.", ErrorCode.BadRequest);


           
            var itemQuantities = dto.RequestItems
                .GroupBy(ri => ri.ItemId)
                .ToDictionary(g => g.Key, g => g.Sum(ri => ri.Quantity ?? 1));

            foreach (var (itemId, totalQuantity) in itemQuantities)
            {
                var item = _itemRepository.GetItemById(itemId);
                if (item == null)
                    throw new ServiceException($"Item {itemId} not found.", ErrorCode.NotFound);

                if (item.Quantity < totalQuantity)
                {
                    throw new ServiceException(
                        $"Insufficient stock for item {item.Name}. " +
                        $"Available: {item.Quantity}, Requested: {totalQuantity}",
                        ErrorCode.BadRequest);
                }
            }

            existingRequest.Description = dto.Description;


            var updatedRequestItems = new List<RequestItemDTO>();

            var newRequestItems = new List<RequestItem>();

            foreach (var reqItemDto in dto.RequestItems)
            {
                var item = _itemRepository.GetItemById(reqItemDto.ItemId);
                if (item == null)
                    throw new ServiceException($"Item {reqItemDto.ItemId} not found.", ErrorCode.NotFound);



                var requestItem = new RequestItem
                {
                    RequestId = id,
                    ItemId = reqItemDto.ItemId,
                    Description = reqItemDto.Description,
                    Quantity = reqItemDto.Quantity ?? 1
                };

                var newRequestItem = _requestItemRepository.CreateRequestItem(requestItem);

                newRequestItems.Add(newRequestItem);

                updatedRequestItems.Add(new RequestItemDTO
                {
                    ItemId = requestItem.ItemId,
                    Description = requestItem.Description,
                    Quantity = requestItem.Quantity
                });
            }


            _requestItemRepository.DeleteItemsByRequestIdExcluding(id, newRequestItems);


            _requestRepository.UpdateRequest(existingRequest);

            return new RequestDTO
            {
                Id = existingRequest.Id,
                Description = existingRequest.Description,
                RequestedByUserId = existingRequest.RequestedByUserId,
                RequestStatus = existingRequest.RequestStatus,
                ReturnIsCompleted = existingRequest.ReturnIsCompleted,
                RequestDate = existingRequest.RequestDate,
                RequestItems = updatedRequestItems
            };
        }


        public void DeleteRequest(Guid id)
        {

            var existingRequest = _requestRepository.GetRequestById(id);
            if (existingRequest == null)
                throw new ServiceException("Request not found.", ErrorCode.NotFound);

            _requestRepository.DeleteRequest(id);
        }


        public RequestDTO CreateRequest(CreateRequestDTO requestDto)
        {
            var user = _userRepository.GetUserById(requestDto.RequestedByUserId);
            if (user == null)
                throw new ServiceException("User not found.", ErrorCode.NotFound);

           

            
            var itemQuantities = requestDto.RequestItems
                .GroupBy(ri => ri.ItemId)
                .ToDictionary(g => g.Key, g => g.Sum(ri => ri.Quantity ?? 1));

            
            foreach (var (itemId, totalQuantity) in itemQuantities)
            {
                var item = _itemRepository.GetItemById(itemId);
                if (item == null)
                    throw new ServiceException($"Item {itemId} not found.", ErrorCode.NotFound);

                if (item.Quantity < totalQuantity)
                {
                    throw new ServiceException(
                        $"Insufficient stock for item {item.Name}. " +
                        $"Available: {item.Quantity}, Requested: {totalQuantity}",
                        ErrorCode.BadRequest);
                }
            }

            
            var newRequest = new Request
            {
                Description = requestDto.Description,
                RequestedByUserId = requestDto.RequestedByUserId,
                RequestStatus = RequestStatus.Pending,
                ReturnIsCompleted = false
            };

            var createdRequest = _requestRepository.CreateRequest(newRequest);

            
            foreach (var reqItem in requestDto.RequestItems)
            {
                var requestItem = new RequestItem
                {
                    RequestId = createdRequest.Id,
                    ItemId = reqItem.ItemId,
                    Description = reqItem.Description,
                    Quantity = reqItem.Quantity ?? 1 
                };

                _requestItemRepository.CreateRequestItem(requestItem);
            }

            return new RequestDTO
            {
                Id = createdRequest.Id,
                Description = createdRequest.Description,
                RequestedByUserId = createdRequest.RequestedByUserId,
                RequestStatus = createdRequest.RequestStatus,
                ReturnIsCompleted = createdRequest.ReturnIsCompleted,
                RequestDate = createdRequest.RequestDate,
                RequestItems = requestDto.RequestItems
            };
        }

        public string ApproveOrRejectRequest(ApproveRejectRequestDTO dto)
        {
            var request = _requestRepository.GetRequestByIdWithoutItem(dto.RequestId);
            if (request == null)
                throw new ServiceException("Request not found.", ErrorCode.NotFound);

            if (request.RequestStatus != RequestStatus.Pending)
                throw new ServiceException("Only pending applications can be approved or rejected.", ErrorCode.BadRequest);

            if (dto.IsApproved)
            {


                var movementTypeOut = _movementTypeRepository.GetMovementTypeByName("borrow");
                if (movementTypeOut == null)
                    throw new ServiceException("Movement type 'borrow' not found.", ErrorCode.NotFound);

                foreach (var reqItem in request.RequestItems)
                {
                    var item = _itemRepository.GetItemById(reqItem.ItemId);

                    if (item == null)
                        throw new ServiceException($"Item {reqItem.ItemId} not found.", ErrorCode.NotFound);



                    item.Quantity -= reqItem.Quantity;
                    _itemRepository.UpdateItem(item);

                    var movement = new Movement
                    {
                        ItemId = item.Id,
                        MovementTypeId = movementTypeOut.Id,
                        Quantity = reqItem.Quantity,
                        Description = $"Borrowed {item.Name} by user {request.RequestedByUserId} on {DateTime.UtcNow}."
                    };

                    _movementRepository.CreateMovement(movement);
                }

                request.RequestStatus = RequestStatus.Approved;
            }
            else
            {
                request.RequestStatus = RequestStatus.Rejected;
            }

            _requestRepository.UpdateRequest(request);
            return dto.IsApproved ? "Request approved and stock updated." : "Request rejected.";
        }



        public string ReturnItems(Guid requestId)
        {
            var request = _requestRepository.GetRequestByIdWithoutItem(requestId);
            if (request == null) throw new ServiceException("Request not found.", ErrorCode.NotFound);

            if (request.RequestStatus != RequestStatus.Approved || request.ReturnIsCompleted)
                throw new ServiceException("Only approved requests can be returned.", ErrorCode.BadRequest);

            var movementTypeIn = _movementTypeRepository.GetMovementTypeByName("return");

            if (movementTypeIn == null) throw new ServiceException("Movement type 'return' not found.", ErrorCode.NotFound);

            foreach (var reqItem in request.RequestItems)
            {
                var item = _itemRepository.GetItemById(reqItem.ItemId);

                if (item == null) throw new ServiceException($"Item {reqItem.ItemId} not found.", ErrorCode.NotFound);

                item.Quantity += reqItem.Quantity;
                _itemRepository.UpdateItem(item);

                var movement = new Movement
                {
                    ItemId = item.Id,
                    MovementTypeId = movementTypeIn.Id,
                    Quantity = reqItem.Quantity,
                    Description = $"Returned {item.Name} by user {request.RequestedByUserId} on {DateTime.UtcNow}."
                };

                _movementRepository.CreateMovement(movement);
            }

            request.ReturnIsCompleted = true;
            request.ReturnDate = DateTime.UtcNow;
            _requestRepository.UpdateRequest(request);

            return "The items were returned correctly.";
        }
    }
}
