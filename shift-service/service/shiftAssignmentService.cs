// using Services;

// public class ShiftAssignmentService
// {
//     private readonly IRabbitMqService _rabbitMqService;
//     private readonly IEmployeeService _employeeService;
//     private readonly ILogger<ShiftAssignmentService> _logger;

//     public async Task AssignShiftsForEvent(EventCreationMessage eventMessage)
//     {
//         // Default role distribution strategy
//         var shiftsToAssign = eventMessage.ShiftsNumber;
        
//         // Always assign 1 head trucker first
//         var headTruckerEmployee = await FindEmployeeForRole("headtrucker");
//         if (headTruckerEmployee == null)
//         {
//             _logger.LogWarning($"No head trucker available for event {eventMessage.EventID}");
//             return;
//         }

//         // Create head trucker shift
//         await CreateShift(new ShiftCreationRequest
//         {
//             EmployeeId = headTruckerEmployee.Id,
//             EventId = eventMessage.EventID,
//             StartTime = eventMessage.StartTime,
//             EndTime = eventMessage.EndTime,
//             ShiftType = "headtrucker"
//         });

//         // Remaining shifts are regular truckers
//         var remainingShifts = shiftsToAssign - 1;
//         var truckerEmployees = await GetEmployeesByRoleAsync("trucker", remainingShifts);

//         foreach (var trucker in truckerEmployees)
//         {
//             await CreateShift(new ShiftCreationRequest
//             {
//                 EmployeeId = trucker.Id,
//                 EventId = eventMessage.EventID,
//                 StartTime = eventMessage.StartTime,
//                 EndTime = eventMessage.EndTime,
//                 ShiftType = "trucker"
//             });
//         }
//     }

//     private async Task<EmployeeDto> FindEmployeeForRole(string roleId)
//     {
//         // Request availability for a single employee of the specified role
//         var availableEmployees = await _employeeService.GetAvailableEmployeesByRole(
//             roleId: roleId,
//             date: eventMessage.StartTime,
//             count: 1
//         );

//         return availableEmployees.FirstOrDefault();
//     }

//     private async Task<List<EmployeeDto>> FindEmployeesForRole(string roleId, int count)
//     {
//         // Request availability for multiple employees of the specified role
//         return await _employeeService.GetAvailableEmployeesByRole(
//             roleId: roleId,
//             date: eventMessage.StartTime,
//             count: count
//         );
//     }

//     private async Task CreateShift(ShiftCreationRequest shiftRequest)
//     {
//         try 
//         {
//             await _shiftRepository.CreateShift(shiftRequest);
            
//             // Publish shift creation event
//             _rabbitMqService.PublishMessage(
//                 exchange: "shift_system",
//                 routingKey: "shift.created",
//                 message: shiftRequest
//             );
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError($"Failed to create shift: {ex.Message}");
//         }
//     }
// }

// // Supporting DTOs
// public class ShiftCreationRequest
// {
//     public string EmployeeId { get; set; }
//     public string EventId { get; set; }
//     public string StartTime { get; set; }
//     public string EndTime { get; set; }
//     public string ShiftType { get; set; }
// }

// public class EmployeeDto
// {
//     public string Id { get; set; }
//     public string RoleId { get; set; }
//     // Other employee details
// }