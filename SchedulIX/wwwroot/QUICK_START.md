# SchedulIX - Quick Start Guide

## 🚀 Getting Started in 5 Minutes

### Step 1: Start the Application
```bash
cd D:\VisualStudio projects\SchedulIX\SchedulIX
dotnet run
```

The application will start on `http://localhost:5000` (or the configured port shown in console).

### Step 2: Open Your Browser
Navigate to:
- **Main Application**: `http://localhost:5000`
- **API Tester**: `http://localhost:5000/test-api.html`
- **API Documentation**: `http://localhost:5000/swagger`

### Step 3: Use the Dashboard

#### 📊 Dashboard Tab
- Overview of system statistics
- Quick access to all features
- System status indicator

#### 🚪 Rooms Management
1. Click "Săli" in navigation
2. Fill the form (Room Number, Capacity, Type)
3. Click "Adăugă Sală"
4. View rooms in the table below

#### 📅 Schedule Generation
1. Click "Orar" in navigation
2. Enter academic year ID (or use default)
3. Enter group IDs (comma-separated)
4. Click "Generează Orar"
5. View results in popup modal

#### 📥 Export Data
1. Click "Export" in navigation
2. Choose format (Excel or PDF)
3. For room-specific export, select room from dropdown
4. File downloads automatically

## 📖 API Testing

### Using test-api.html

1. Open `http://localhost:5000/test-api.html`
2. Select category from tabs:
   - **Săli** (Rooms)
   - **Orare** (Schedules)
   - **Export** (Exports)
   - **Documentație** (Documentation)

3. Enter required parameters
4. Click "Testează" (Test)
5. View JSON response

### Example: Get All Rooms
1. Go to "Săli" tab
2. Click "Testează" for "GET /api/rooms"
3. See response with room list

### Example: Create Room
1. Go to "Săli" tab
2. Fill in:
   - Numărul Sălii: "202"
   - Capacitate: "45"
   - Tip Sală: "Laborator"
3. Click "Testează"
4. Success response shown

## 🔑 Common Tasks

### Add a New Room
```
Navigation: Săli (Rooms)
Form:
  - Room Number: 103
  - Capacity: 50
  - Type: Curs (Course)
Button: Adăugă Sală
```

### View Room Schedule
```
Navigation: Săli
Table: Click calendar icon on room row
Result: Modal shows scheduled classes
```

### Generate Schedule
```
Navigation: Orar
Form:
  - Academic Year: [copy from field or use default]
  - Groups: [paste UUIDs or IDs]
  - Soft Constraints: [optional checkbox]
Button: Generează Orar
Result: Schedule modal with details
```

### Download Schedule as Excel
```
Navigation: Export
Section: "Export hacia Excel"
Button: Descarcă Excel
Result: File downloads to Downloads folder
```

### Download Schedule as PDF
```
Navigation: Export
Section: "Export hacia PDF"
Button: Descarcă PDF
Result: File downloads to Downloads folder
```

### Export Specific Room's Calendar
```
Navigation: Export
Section: "Export Calendar Sală"
Dropdown: Select room
Button: Export Calendar Sală (Excel)
Result: Excel file with room's schedule
```

## 🔧 Troubleshooting

### Issue: Blank White Page
**Solution:**
1. Press F12 to open Developer Tools
2. Check Console tab for errors
3. Verify server is running (check console output)
4. Refresh page (Ctrl+R)

### Issue: API Buttons Say "Not Found"
**Solution:**
1. Verify backend is running
2. Check port number (default 5000)
3. Ensure database is accessible
4. Check appsettings.json for connection string

### Issue: Download Not Working
**Solution:**
1. Check browser download setting
2. Ensure file size is reasonable (<100MB)
3. Try different browser
4. Clear browser cache (Ctrl+Shift+Delete)

### Issue: Form Submission Fails
**Solution:**
1. Check all required fields are filled
2. Open DevTools Network tab (F12)
3. Look for HTTP error codes (400, 500, etc.)
4. Check console for JavaScript errors
5. Verify data format matches API expectations

## 📝 File Locations

```
SchedulIX/
├── SchedulIX.slnx              # Solution file
├── SchedulIX.csproj            # Project file
├── Program.cs                  # Application startup
├── appsettings.json            # Configuration
├── wwwroot/                    # Static files (frontend)
│   ├── index.html              # Main application
│   ├── test-api.html           # API testing tool
│   ├── FRONTEND_README.md      # Frontend documentation
│   ├── IMPLEMENTATION_SUMMARY.md
│   └── js/
│       ├── api-client.js       # API wrapper
│       └── main.js             # UI logic
├── Controllers/
│   ├── RoomsController.cs      # Room endpoints
│   ├── SchedulesController.cs  # Schedule endpoints
│   └── ExportController.cs     # Export endpoints
└── Data/
	└── ScheduleDbContext.cs    # Database context
```

## 🌐 API Endpoints Summary

### Rooms (6 endpoints)
| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/rooms` | Get all rooms |
| GET | `/api/rooms/{id}` | Get room details |
| GET | `/api/rooms/{id}/calendar` | Get room schedule |
| POST | `/api/rooms` | Create new room |
| PUT | `/api/rooms/{id}` | Update room |
| PATCH | `/api/rooms/{id}/status` | Change room status |

### Schedules (3 endpoints)
| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/schedules/generate` | Generate new schedule |
| GET | `/api/schedules/{id}` | Get schedule details |
| POST | `/api/schedules/validate` | Validate schedule |

### Export (3 endpoints)
| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/export/excel` | Export to Excel |
| GET | `/api/export/pdf` | Export to PDF |
| GET | `/api/export/room/{id}/excel` | Export room calendar |

## 💻 Developer Quick Reference

### Add API Method to Client
```javascript
// In wwwroot/js/api-client.js
async myNewMethod(parameters) {
	return this.fetch('/endpoint', {
		method: 'POST',
		body: JSON.stringify(parameters)
	});
}
```

### Add UI Button Event
```javascript
// In wwwroot/js/main.js
document.getElementById('myButton').addEventListener('click', async () => {
	try {
		const result = await apiClient.myNewMethod(data);
		showAlert('success', 'Success!');
	} catch (error) {
		showAlert('danger', error.message);
	}
});
```

### Show Alert Message
```javascript
// Green success
showAlert('success', 'Operation completed!');

// Red error
showAlert('danger', 'An error occurred!');

// Blue info
showAlert('info', 'Here is some information');

// Yellow warning
showAlert('warning', 'Please be careful');
```

### Navigate to Page
```javascript
showPage('rooms');      // Show rooms page
showPage('schedule');   // Show schedule page
showPage('export');     // Show export page
showPage('dashboard');  // Show dashboard
```

## 📱 Responsive Design

The application works on:
- ✅ Desktop (1920px, 1366px, 1024px)
- ✅ Tablet (768px - iPad)
- ✅ Mobile (375px - 568px)
- ✅ Large Mobile (412px - 915px)

Everything is mobile-friendly with:
- Hamburger menu on small screens
- Stacked layout on mobile
- Touch-friendly buttons
- Readable text sizes

## 🎨 Customization

### Change Color Theme
Edit in `wwwroot/index.html` `<style>` section:
```css
:root {
	--primary-color: #667eea;      /* Change to your color */
	--success-color: #198754;
	--danger-color: #dc3545;
}
```

### Customize Navbar
Edit navbar HTML in `index.html`:
```html
<span class="navbar-brand">Your Brand Name</span>
```

### Add New Navigation Item
```html
<li class="nav-item">
	<a class="nav-link" href="#" onclick="showPage('yourpage')">
		<i class="fas fa-icon-name"></i> Your Page
	</a>
</li>
```

## 📊 Screenshot Description

### Main Dashboard
- Large purple gradient header with "SchedulIX" branding
- Dark navbar with navigation links
- 4 stat cards: Rooms, Schedules, Status, Export
- System information panel below

### Rooms Page
- Form to add new room (30% width)
- Table with room list and action buttons (70% width)
- Reload button to refresh data

### Schedule Page
- Large form to generate schedule (top)
- Table showing generated schedules (bottom)
- Modal popup on generation completion

### Export Page
- Two large cards: Excel export, PDF export
- Dropdown selector for room-specific export
- Download buttons on each section

### API Tester
- 5 tab categories
- Input fields for parameters
- JSON response preview
- Copy-to-clipboard ready

## 🆘 Getting Help

1. **Check Documentation**
   - `wwwroot/FRONTEND_README.md` - Frontend guide
   - `wwwroot/IMPLEMENTATION_SUMMARY.md` - Implementation details
   - This file - Quick start

2. **Use Swagger UI**
   - Navigate to `/swagger`
   - Interactive API documentation
   - Try requests directly in browser

3. **Use API Tester**
   - Navigate to `/test-api.html`
   - Test endpoints with UI
   - See formatted responses

4. **Check Browser Console**
   - Press F12
   - Go to Console tab
   - Look for JavaScript errors

5. **Check Network Tab**
   - Press F12
   - Go to Network tab
   - Make request
   - Check response status and body

## ⚡ Performance Tips

- First page load might take a moment (static assets loading)
- Schedule generation takes time (optimization algorithm running)
- Subsequent requests are fast (API caching enabled)
- Large exports might take 10-30 seconds
- Close unused browser tabs to free memory

## 📞 Contact & Support

**For Issues:**
1. Check browser console (F12)
2. Review error message in alert
3. Try again after clearing cache
4. Check API connectivity with status button

**Log Locations:**
- Server logs: Console output when running `dotnet run`
- Browser logs: Developer Console (F12)
- Request logs: Network tab (F12)

---

**Happy Scheduling! 🎉**

Need more help? Use the test-api.html tool to explore all available endpoints.
