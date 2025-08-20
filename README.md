# ElasoftCommunityManagementSystem

A comprehensive community management system built with ASP.NET Core backend and Vue.js frontend for managing university clubs and communities.

## 🆕 New Feature: Single President Per Club Constraint

### Overview

This system now ensures that **only one president can be assigned to a single community/club** at any time. This feature includes database-level constraints, API endpoints, and comprehensive validation to maintain data integrity.

### 🔧 Implementation Details

#### 1. Database-Level Constraint

- **Unique Partial Index**: `IX_ClubMembership_ClubId_President`
- **Purpose**: Prevents multiple presidents per club at the database level
- **Constraint**: Only one approved membership with role "başkan" can exist per club
- **Migration**: `20250704162312_AddUniqueConstraintForClubPresident`

```sql
CREATE UNIQUE INDEX IX_ClubMembership_ClubId_President
ON ClubMembership (ClubId)
WHERE Status = 'approved' AND Role = 'başkan'
```

#### 2. Enhanced Service Methods

##### `SetClubLeaderAsync(int membershipId, int userId)`

- **Transaction-based**: Uses database transactions to prevent race conditions
- **Automatic demotion**: Current president is automatically demoted to "member" role
- **Validation**: Checks if user is already a president before assignment
- **Notifications**: Sends notifications to both new and old presidents
- **Authorization**: Only admins and advisors can set club leaders

##### `ValidateClubPresidentUniquenessAsync(int clubId)`

- **Purpose**: Validates that a club has exactly one president
- **Returns**: Boolean indicating if the club has valid president count
- **Usage**: Data integrity checks and validation

##### `GetClubPresidentAsync(int clubId)`

- **Purpose**: Retrieves the current president of a club
- **Returns**: President's membership information or null if no president exists
- **Includes**: User details and membership information

#### 3. New API Endpoints

##### Set Club Leader

```http
PUT /api/memberships/set-leader/{membershipId}
Authorization: Bearer {token}
```

- **Purpose**: Sets a club member as the club president
- **Authorization**: Admin or Advisor roles only
- **Response**: Success message or error details

##### Validate Club President

```http
GET /api/memberships/validate-president/{clubId}
Authorization: Bearer {token}
```

- **Purpose**: Validates club president uniqueness
- **Returns**: Validation result and current president information
- **Response Format**:

```json
{
  "isValid": true,
  "hasPresident": true,
  "president": {
    "userId": 123,
    "name": "John",
    "surname": "Doe",
    "role": "başkan",
    "joinedAt": "2024-01-15T10:30:00Z"
  }
}
```

### 🛡️ Security Features

#### Authorization Rules

- **Set Leader**: Only users with "admin" or "advisor" roles
- **Validate President**: All authenticated users
- **Authentication**: JWT token required for all endpoints

#### Validation Checks

- User must be authenticated
- Membership must exist and be approved
- User must have appropriate role permissions
- Prevents setting same user as president twice

### 🔄 Business Logic Flow

#### Setting a New President

1. **Validation**: Check if membership exists and is approved
2. **Authorization**: Verify user has admin/advisor role
3. **Duplicate Check**: Ensure user isn't already president
4. **Transaction Start**: Begin database transaction
5. **Demote Current**: Change current president to "member" role
6. **Promote New**: Set new user as "başkan"
7. **Save Changes**: Commit transaction
8. **Notifications**: Send notifications to affected users

#### President Validation

1. **Count Check**: Verify exactly one president exists
2. **Status Check**: Ensure president has "approved" status
3. **Role Check**: Confirm role is "başkan"
4. **Return Result**: Provide validation status and president details

### 📊 Database Schema Changes

#### ClubMembership Table

- **Unique Index**: Added partial unique index on ClubId for presidents
- **Constraint**: Prevents multiple presidents per club
- **Performance**: Optimized queries for president lookups

### 🚀 API Documentation

The API is fully documented with Swagger/OpenAPI. Access the documentation at:

```
https://localhost:7274/swagger/index.html
```

#### Key Endpoints Added:

- `PUT /api/memberships/set-leader/{membershipId}` - Set club leader
- `GET /api/memberships/validate-president/{clubId}` - Validate president

### 🔧 Technical Implementation

#### Transaction Management

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Business logic here
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

#### Notification System Integration

```csharp
// Notify new president
await _notificationService.CreateNotificationAsync(
    membership.UserId,
    "Başkanlık Atama",
    $"Tebrikler! {membership.Club?.Name} başkanı olarak atandınız.",
    "membership"
);

// Notify old president
await _notificationService.CreateNotificationAsync(
    currentLeader.UserId,
    "Başkanlık Devri",
    $"{membership.Club?.Name} başkanlığınız başka bir üyeye devredildi.",
    "membership"
);
```

### 🎯 Benefits

#### Data Integrity

- **Database-level enforcement** prevents corruption
- **Transaction-based operations** ensure consistency
- **Validation at multiple levels** (API, service, database)

#### User Experience

- **Clear notifications** for president changes
- **Proper error messages** for invalid operations
- **Smooth transitions** when changing leadership

#### System Reliability

- **Race condition prevention** through transactions
- **Comprehensive error handling** with meaningful messages
- **Authorization checks** prevent unauthorized access

### 🧪 Testing

#### Manual Testing

1. **Set President**: Use the API endpoint to set a club leader
2. **Validate**: Check that only one president exists per club
3. **Duplicate Prevention**: Try to set multiple presidents (should fail)
4. **Notifications**: Verify notifications are sent correctly

#### Database Testing

1. **Constraint Test**: Try to insert multiple presidents directly in database
2. **Index Performance**: Verify query performance with the new index
3. **Migration Test**: Ensure migration applies correctly

### 📝 Usage Examples

#### Setting a Club Leader (cURL)

```bash
curl -X PUT "https://localhost:7274/api/memberships/set-leader/123" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

#### Validating Club President (cURL)

```bash
curl -X GET "https://localhost:7274/api/memberships/validate-president/456" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 🔮 Future Enhancements

#### Potential Improvements

- **President History**: Track previous presidents
- **Term Limits**: Add support for presidential terms
- **Automatic Elections**: Implement voting system for president selection
- **Deputy President**: Add support for vice-president roles

#### Monitoring

- **Audit Logs**: Track all president changes
- **Analytics**: Monitor club leadership patterns
- **Alerts**: Notify admins of leadership changes

### 🏗️ Project Structure

```
Backend/ElasoftCommunityManagementSystem/
├── Controllers/
│   └── ClubMembershipController.cs      # New endpoints added
├── Services/
│   └── ClubMembershipService.cs         # Enhanced with new methods
├── Interfaces/
│   └── IClubMembershipService.cs        # New method signatures
├── Migrations/
│   └── 20250704162312_AddUniqueConstraintForClubPresident.cs
└── Models/
    └── ClubMembershipModel.cs           # Existing model with new constraints
```

### 🚀 Getting Started

#### Prerequisites

- .NET 8.0 SDK
- SQL Server
- Entity Framework Core Tools

#### Running the Application

1. **Database Setup**:

   ```bash
   dotnet ef database update
   ```

2. **Run Backend**:

   ```bash
   cd Backend/ElasoftCommunityManagementSystem
   dotnet run
   ```

3. **Access API Documentation**:
   ```
   https://localhost:7274/swagger/index.html
   ```

### 📞 Support

For questions or issues related to the president uniqueness feature:

- Check the API documentation at `/swagger/index.html`
- Review the service methods in `ClubMembershipService.cs`
- Validate database constraints in the migration files

---

## Original Project Features

[Include your existing project documentation here...]

## Technology Stack

- **Backend**: ASP.NET Core 8.0
- **Frontend**: Vue.js 3
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: JWT
- **API Documentation**: Swagger/OpenAPI

## License

[Your license information here]
