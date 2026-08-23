# SchedulIX Frontend Documentation

## Overview

The SchedulIX frontend is a modern, responsive web application built with Bootstrap 5 that provides a user-friendly interface to interact with the SchedulIX REST API. The application is structured as a single-page application with multiple views.

## File Structure

```
wwwroot/
├── index.html              # Main dashboard and application shell
├── test-api.html          # API testing and documentation tool
├── js/
│   ├── api-client.js      # Centralized API client wrapper
│   └── main.js            # UI logic and event handlers
└── css/
	└── (Bootstrap CDN used)
```

## Pages & Features

### 1. **index.html** - Main Dashboard
The primary application interface with 5 main sections:

#### Dashboard Section
- Overview cards showing:
  - Total number of rooms
  - Number of schedules
  - API connection status
  - Export options
- Quick access buttons to all features
- System status information

#### Rooms Management
- **List All Rooms**: Display table with room details
  - Room number, capacity, type, and availability status
  - Action buttons: View calendar, toggle availability
- **Add New Room**: Form to create new rooms with:
  - Room number
  - Capacity
  - Room type (Course/Lab/Seminar)
- **Room Calendar View**: Modal showing scheduled classes for a room

#### Schedule Generation & Management
- **Auto-Generate Schedule**: Form to create schedules with:
  - Academic year selection
  - Student groups selection
  - Soft constraint options
- **View Schedules**: Table of generated schedules showing:
  - Schedule ID and name
  - Creation date
  - Hard constraint violations
  - Soft constraint score
- **Detailed Schedule View**: Modal with full schedule details

#### Export Section
- **Full Schedule Export**:
  - Export to Excel (.xlsx)
  - Export to PDF (.pdf)
- **Room Calendar Export**:
  - Select specific room
  - Export calendar to Excel

#### API Documentation
- Comprehensive endpoint reference
- All 11 API endpoints documented with:
  - HTTP method
  - Endpoint path
  - Brief description
- Links to Swagger UI

### 2. **test-api.html** - API Tester
Interactive tool for testing all API endpoints with live response previews.

**Features:**
- Tab-based interface for different endpoint categories
- **Rooms Tab**: Test room endpoints with UI for parameters
- **Schedules Tab**: Test schedule generation and validation
- **Export Tab**: Download exports directly
- **Documentation Tab**: Complete API reference with method types
- Real-time response preview in formatted JSON
- Alert notifications for success/error responses

## JavaScript Modules

### api-client.js
Centralized API client for all backend communication.

**Class: `ScheduleAPIClient`**

**Room Endpoints:**
```javascript
// Get all rooms
const rooms = await apiClient.getRooms();

// Get room by ID
const room = await apiClient.getRoom(roomId);

// Get room calendar
const calendar = await apiClient.getRoomCalendar(roomId);

// Create room
const newRoom = await apiClient.createRoom({
	roomNumber: "201",
	capacity: 50,
	roomTypeId: 1,
	availabilities: []
});

// Update room
await apiClient.updateRoom(roomId, roomData);

// Update room status
await apiClient.updateRoomStatus(roomId, isAvailable);
```

**Schedule Endpoints:**
```javascript
// Generate schedule
const schedule = await apiClient.generateSchedule({
	academicYearId: "uuid",
	groupIds: ["id1", "id2"],
	allowSoftConstraintViolations: false
});

// Get schedule by ID
const schedule = await apiClient.getSchedule(scheduleId);

// Validate schedule
const result = await apiClient.validateSchedule(scheduleData);
```

**Export Endpoints:**
```javascript
// Export to Excel
const response = await apiClient.exportToExcel();

// Export to PDF
const response = await apiClient.exportToPdf();

// Export room schedule to Excel
const response = await apiClient.exportRoomSchedule(roomId);
```

### main.js
Contains all UI logic, event handlers, and page navigation.

**Key Functions:**
- `showPage(pageId)`: Navigate between sections
- `showAlert(type, message)`: Display notifications
- `loadDashboardData()`: Initialize dashboard
- `loadRooms()`: Load and render room list
- `loadSchedules()`: Load and render schedules
- `loadRoomsForExport()`: Populate export dropdown
- `exportToExcel()`: Download full schedule as Excel
- `exportToPdf()`: Download full schedule as PDF
- `checkApiStatus()`: Verify API connectivity

**Event Listeners:**
- Form submissions (add room, generate schedule, export)
- Button clicks (toggle room status, view calendar)
- Navigation links

## Styling & UI

### Design Features
- **Color Scheme**: Purple gradient theme (#667eea to #764ba2)
- **Responsive**: Bootstrap 5 grid system for mobile/tablet/desktop
- **Icons**: Font Awesome 6.4 for visual elements
- **Animations**: Smooth transitions and hover effects
- **Accessibility**: ARIA labels and semantic HTML

### Key UI Components
- Navbar with navigation
- Card-based layout for content sections
- Responsive tables with hover effects
- Modal dialogs for detailed views
- Alert notifications for feedback
- Loading spinner overlay
- Forms with validation

## API Integration

### Base URL
The application automatically detects the API base URL:
```javascript
const API_BASE_URL = window.location.origin;
const API_ENDPOINT = API_BASE_URL + '/api';
```

### Error Handling
All API calls include:
- Try-catch error handling
- User-friendly error messages via alerts
- Loading state management
- HTTP error detection

### File Downloads
File downloads (Excel, PDF) are handled with:
- Blob creation from response
- Automatic file naming with timestamps
- Browser's native download mechanism

## Usage Guide

### For Users

1. **Viewing Dashboard**
   - Open `index.html` in browser
   - Dashboard loads automatically with current data

2. **Managing Rooms**
   - Click "Săli" in navigation
   - Add new room with the form
   - View existing rooms in table
   - Click calendar icon to see room schedule

3. **Generating Schedules**
   - Click "Orar" in navigation
   - Fill academic year and group IDs
   - Click "Generează Orar"
   - View results in modal dialog

4. **Exporting Data**
   - Click "Export" in navigation
   - Choose export format (Excel/PDF)
   - Select room for room-specific export
   - Files download automatically

5. **Testing APIs**
   - Open `test-api.html`
   - Select endpoint category from tabs
   - Enter parameters if needed
   - Click "Testează" to execute
   - View formatted JSON response

### For Developers

1. **Adding New Endpoints**
   - Add method to `ScheduleAPIClient` class
   - Use consistent parameter naming
   - Return parsed JSON responses
   - Document with JSDoc comments

2. **Adding New Pages**
   - Create new section div in HTML with `page-section` class
   - Add navigation link with `showPage()` onclick
   - Implement page load function in `main.js`
   - Style using existing CSS classes

3. **Testing New Features**
   - Use `test-api.html` for quick testing
   - Check browser console for API errors
   - Use browser DevTools Network tab to inspect requests
   - Verify response status and content

## Browser Compatibility

- Chrome/Chromium: Full support
- Firefox: Full support
- Safari: Full support
- Edge: Full support
- IE 11: Not supported (uses modern JavaScript)

## Performance Considerations

- Static assets hosted by Kestrel server
- Bootstrap & Font Awesome from CDN
- Single JS client instance for all API calls
- No external frontend framework (Vanilla JS)
- Minimal dependencies for fast loading

## Security Notes

- HTML encoding to prevent XSS attacks
- CORS configuration in API (Program.cs)
- No sensitive data stored in client storage
- API calls use application/json content type
- Same-origin requests only

## Troubleshooting

### API Returns 404
- Verify backend is running
- Check API endpoint paths
- Verify room/schedule IDs exist

### Download Not Working
- Check browser download permissions
- Verify file size limits
- Check browser console for errors

### Form Submission Fails
- Verify all required fields filled
- Check browser console for validation errors
- Ensure API server is responding

### Responsive Layout Issues
- Clear browser cache (Ctrl+Shift+Delete)
- Check Bootstrap CDN is loading
- Inspect viewport meta tag in HTML

## Future Enhancements

Potential features for future development:
- User authentication and authorization
- Real-time schedule updates via WebSocket
- Drag-and-drop schedule editing
- Advanced filtering and search
- Batch import from CSV
- Calendar integration
- Mobile app

## Support

For API documentation, access the Swagger UI at: `/swagger`

For issues or questions, check:
1. Browser console (F12) for errors
2. Network tab for API responses
3. Backend logs for server errors
4. API documentation in test-api.html
