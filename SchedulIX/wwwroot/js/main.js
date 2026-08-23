/**
 * SchedulIX Frontend Main JavaScript
 * Handles UI interactions and API calls
 */

// ===== UTILITY FUNCTIONS =====

/**
 * Show/hide loading spinner
 */
function showLoading(show = true) {
    const spinner = document.getElementById('loadingSpinner');
    const overlay = document.getElementById('overlay');

    if (show) {
        spinner.classList.add('active');
        overlay.classList.add('active');
    } else {
        spinner.classList.remove('active');
        overlay.classList.remove('active');
    }
}

/**
 * Show alert message
 */
function showAlert(type, message) {
    const container = document.getElementById('alertContainer');
    const alertId = 'alert-' + Date.now();

    const alert = document.createElement('div');
    alert.id = alertId;
    alert.className = `alert alert-${type} alert-dismissible fade show`;
    alert.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    container.appendChild(alert);

    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        const element = document.getElementById(alertId);
        if (element) {
            element.remove();
        }
    }, 5000);
}

/**
 * Navigate between pages
 */
function showPage(pageId) {
    // Hide all pages
    document.querySelectorAll('.page-section').forEach(section => {
        section.classList.remove('active');
    });

    // Show selected page
    const page = document.getElementById(pageId);
    if (page) {
        page.classList.add('active');
    }

    // Update nav links
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });
    event.target.closest('.nav-link')?.classList.add('active');

    // Load data for the page
    if (pageId === 'rooms') {
        loadRooms();
    } else if (pageId === 'schedule') {
        loadSchedules();
    } else if (pageId === 'export') {
        loadRoomsForExport();
    } else if (pageId === 'dashboard') {
        loadDashboardData();
    }
}

/**
 * Format date to readable string
 */
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('ro-RO', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

/**
 * Get room type name
 */
function getRoomTypeName(typeId) {
    const types = {
        1: 'Curs',
        2: 'Laborator',
        3: 'Seminar'
    };
    return types[typeId] || 'Necunoscut';
}

// ===== PAGE: DASHBOARD =====

async function loadDashboardData() {
    try {
        showLoading(true);

        // Load rooms count
        const rooms = await apiClient.getRooms();
        const roomCount = Array.isArray(rooms) ? rooms.length : 0;
        document.getElementById('roomCount').textContent = roomCount;

        showAlert('info', 'Dashboard actualizat cu succes!');
    } catch (error) {
        showAlert('danger', `Eroare la încărcare dashboard: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

async function checkApiStatus() {
    try {
        showLoading(true);

        const rooms = await apiClient.getRooms();
        const statusElement = document.getElementById('statusText');

        if (Array.isArray(rooms)) {
            statusElement.textContent = 'Conectat';
            statusElement.className = 'badge bg-success';
            showAlert('success', 'API este conectat și funcțional!');
        }
    } catch (error) {
        document.getElementById('statusText').textContent = 'Deconectat';
        document.getElementById('statusText').className = 'badge bg-danger';
        showAlert('danger', `API este inaccesibil: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

// ===== PAGE: ROOMS MANAGEMENT =====

async function loadRooms() {
    try {
        showLoading(true);

        const rooms = await apiClient.getRooms();
        const tableBody = document.getElementById('roomsTableBody');

        if (!Array.isArray(rooms) || rooms.length === 0) {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="5" class="text-center text-muted">
                        Nu sunt săli în bază de date
                    </td>
                </tr>
            `;
            return;
        }

        tableBody.innerHTML = rooms.map(room => `
            <tr>
                <td>
                    <strong>${escapeHtml(room.roomNumber)}</strong>
                </td>
                <td>
                    <strong>${room.capacity}</strong> locuri
                </td>
                <td>
                    <span class="badge bg-info">
                        ${getRoomTypeName(room.roomType?.id)}
                    </span>
                </td>
                <td>
                    ${room.isAvailable 
                        ? '<span class="badge bg-success">Disponibilă</span>'
                        : '<span class="badge bg-danger">Indisponibilă</span>'
                    }
                </td>
                <td>
                    <button class="btn btn-sm btn-info" onclick="viewRoomCalendar(${room.id})" title="Calendar">
                        <i class="fas fa-calendar"></i>
                    </button>
                    <button class="btn btn-sm btn-warning" onclick="toggleRoomStatus(${room.id}, ${!room.isAvailable})" title="Schimbă status">
                        <i class="fas fa-sync"></i>
                    </button>
                </td>
            </tr>
        `).join('');

    } catch (error) {
        showAlert('danger', `Eroare la încărcare săli: ${error.message}`);
        document.getElementById('roomsTableBody').innerHTML = `
            <tr>
                <td colspan="5" class="text-center text-danger">
                    Eroare la încărcare: ${escapeHtml(error.message)}
                </td>
            </tr>
        `;
    } finally {
        showLoading(false);
    }
}

async function viewRoomCalendar(roomId) {
    try {
        showLoading(true);

        const calendar = await apiClient.getRoomCalendar(roomId);

        let html = `<h5>Calendar Sală ${escapeHtml(calendar.roomNumber)}</h5>`;

        if (calendar.scheduledItems && calendar.scheduledItems.length > 0) {
            html += '<div class="table-responsive"><table class="table table-sm">';
            html += '<thead><tr><th>Disciplina</th><th>Profesor</th><th>Grupa</th><th>Ziua</th><th>Ora</th></tr></thead><tbody>';

            calendar.scheduledItems.forEach(item => {
                html += `
                    <tr>
                        <td>${escapeHtml(item.disciplineName)}</td>
                        <td>${escapeHtml(item.teacherName)}</td>
                        <td>${escapeHtml(item.groupName)}</td>
                        <td>${getDayName(item.dayOfWeek)}</td>
                        <td>${item.startTime} - ${item.endTime}</td>
                    </tr>
                `;
            });

            html += '</tbody></table></div>';
        } else {
            html += '<p class="text-muted">Nu sunt clase programate pentru această sală</p>';
        }

        showModal('Calendar Sală', html);

    } catch (error) {
        showAlert('danger', `Eroare la încărcare calendar: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

async function toggleRoomStatus(roomId, newStatus) {
    try {
        showLoading(true);

        await apiClient.updateRoomStatus(roomId, newStatus);

        showAlert('success', `Status sălii a fost actualizat!`);
        await loadRooms();

    } catch (error) {
        showAlert('danger', `Eroare la actualizare status: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

// Add Room Form Submit
document.addEventListener('DOMContentLoaded', function() {
    const addRoomForm = document.getElementById('addRoomForm');
    if (addRoomForm) {
        addRoomForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            try {
                showLoading(true);

                const roomData = {
                    roomNumber: document.getElementById('roomNumber').value,
                    capacity: parseInt(document.getElementById('roomCapacity').value),
                    roomTypeId: parseInt(document.getElementById('roomType').value),
                    availabilities: []
                };

                await apiClient.createRoom(roomData);

                showAlert('success', 'Sala a fost adăugată cu succes!');
                addRoomForm.reset();
                await loadRooms();

            } catch (error) {
                showAlert('danger', `Eroare la adăugare sală: ${error.message}`);
            } finally {
                showLoading(false);
            }
        });
    }
});

// ===== PAGE: SCHEDULES MANAGEMENT =====

async function loadSchedules() {
    try {
        showLoading(true);

        // For now, show empty table with instructions
        const tableBody = document.getElementById('schedulesTableBody');
        tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted">
                    <p>Utilizează formularul de mai sus pentru a genera un orar nou</p>
                    <small>Orare generate vor apărea în această tabelă</small>
                </td>
            </tr>
        `;

    } finally {
        showLoading(false);
    }
}

// Generate Schedule Form Submit
document.addEventListener('DOMContentLoaded', function() {
    const generateForm = document.getElementById('generateScheduleForm');
    if (generateForm) {
        generateForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            try {
                showLoading(true);

                const academicYearId = document.getElementById('academicYear').value;
                const groupIdsInput = document.getElementById('groupIds').value;
                const allowSoftConstraints = document.getElementById('allowSoftConstraints').checked;

                // Parse group IDs
                const groupIds = groupIdsInput.split(',').map(id => id.trim()).filter(id => id.length > 0);

                if (groupIds.length === 0) {
                    showAlert('warning', 'Introduceți cel puțin o grupă de studenți');
                    showLoading(false);
                    return;
                }

                const scheduleRequest = {
                    academicYearId: academicYearId,
                    groupIds: groupIds,
                    allowSoftConstraintViolations: allowSoftConstraints
                };

                const result = await apiClient.generateSchedule(scheduleRequest);

                showAlert('success', `Orar generat cu succes! ID: ${result.id}`);

                // Show schedule details
                showScheduleDetails(result);

                // Reload schedules list
                await loadSchedules();

            } catch (error) {
                showAlert('danger', `Eroare la generare orar: ${error.message}`);
            } finally {
                showLoading(false);
            }
        });
    }
});

function showScheduleDetails(schedule) {
    let html = `
        <h5>Detalii Orar</h5>
        <div class="row mb-3">
            <div class="col-md-6">
                <p><strong>ID:</strong> ${escapeHtml(schedule.id)}</p>
                <p><strong>Nume:</strong> ${escapeHtml(schedule.name)}</p>
                <p><strong>Data creare:</strong> ${formatDate(schedule.createdAt)}</p>
            </div>
            <div class="col-md-6">
                <p><strong>Încălcări constrângeri hard:</strong> 
                    ${schedule.hardConstraintViolations > 0 
                        ? `<span class="badge bg-danger">${schedule.hardConstraintViolations}</span>`
                        : `<span class="badge bg-success">0</span>`
                    }
                </p>
                <p><strong>Score constrângeri soft:</strong> 
                    <span class="badge bg-info">${schedule.softConstraintScore}/100</span>
                </p>
            </div>
        </div>

        <h6>Clase Programate:</h6>
        <div class="table-responsive">
            <table class="table table-sm">
                <thead>
                    <tr>
                        <th>Disciplina</th>
                        <th>Profesor</th>
                        <th>Grupa</th>
                        <th>Sala</th>
                        <th>Ziua</th>
                        <th>Ora</th>
                    </tr>
                </thead>
                <tbody>
                    ${schedule.items.map(item => `
                        <tr>
                            <td>${escapeHtml(item.subjectName)}</td>
                            <td>${escapeHtml(item.teacherName)}</td>
                            <td>${escapeHtml(item.groupName)}</td>
                            <td>${escapeHtml(item.roomName)}</td>
                            <td>${getDayName(item.day)}</td>
                            <td>${item.startTime} - ${item.endTime}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;

    showModal('Orar Generat', html);
}

// ===== PAGE: EXPORT =====

async function loadRoomsForExport() {
    try {
        const rooms = await apiClient.getRooms();
        const select = document.getElementById('roomSelectExport');

        if (!Array.isArray(rooms) || rooms.length === 0) {
            select.innerHTML = '<option value="">Nu sunt săli disponibile</option>';
            return;
        }

        select.innerHTML = '<option value="">Selectează sală...</option>';
        rooms.forEach(room => {
            const option = document.createElement('option');
            option.value = room.id;
            option.textContent = `Sala ${room.roomNumber} (${room.capacity} locuri)`;
            select.appendChild(option);
        });

    } catch (error) {
        showAlert('danger', `Eroare la încărcare săli: ${error.message}`);
    }
}

async function exportToExcel() {
    try {
        showLoading(true);

        const response = await fetch(`${API_BASE_URL}/api/export/excel`);

        if (!response.ok) {
            throw new Error(`Eroare HTTP ${response.status}`);
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Orar_${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        showAlert('success', 'Orarul a fost descarcate în format Excel!');

    } catch (error) {
        showAlert('danger', `Eroare la export Excel: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

async function exportToPdf() {
    try {
        showLoading(true);

        const response = await fetch(`${API_BASE_URL}/api/export/pdf`);

        if (!response.ok) {
            throw new Error(`Eroare HTTP ${response.status}`);
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Orar_${new Date().toISOString().split('T')[0]}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        showAlert('success', 'Orarul a fost descarcate în format PDF!');

    } catch (error) {
        showAlert('danger', `Eroare la export PDF: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

// Export Room Schedule
document.addEventListener('DOMContentLoaded', function() {
    const exportRoomForm = document.getElementById('exportRoomForm');
    if (exportRoomForm) {
        exportRoomForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const roomId = document.getElementById('roomSelectExport').value;

            if (!roomId) {
                showAlert('warning', 'Selectați o sală');
                return;
            }

            try {
                showLoading(true);

                const response = await fetch(`${API_BASE_URL}/api/export/room/${roomId}/excel`);

                if (!response.ok) {
                    throw new Error(`Eroare HTTP ${response.status}`);
                }

                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = `Calendar_Sala_${roomId}_${new Date().toISOString().split('T')[0]}.xlsx`;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                window.URL.revokeObjectURL(url);

                showAlert('success', 'Calendarul sălii a fost descarcate!');

            } catch (error) {
                showAlert('danger', `Eroare la export: ${error.message}`);
            } finally {
                showLoading(false);
            }
        });
    }
});

// ===== HELPER FUNCTIONS =====

/**
 * Get day name from number
 */
function getDayName(dayOfWeek) {
    const days = {
        0: 'Duminică',
        1: 'Luni',
        2: 'Marți',
        3: 'Miercuri',
        4: 'Joi',
        5: 'Vineri',
        6: 'Sâmbătă'
    };
    return days[dayOfWeek] || 'Necunoscut';
}

/**
 * Escape HTML to prevent XSS
 */
function escapeHtml(text) {
    if (!text) return '';
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

/**
 * Show modal dialog
 */
function showModal(title, content) {
    const modalHtml = `
        <div class="modal fade" id="dynamicModal" tabindex="-1">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${escapeHtml(title)}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        ${content}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            Închide
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Remove old modal if exists
    const oldModal = document.getElementById('dynamicModal');
    if (oldModal) {
        oldModal.remove();
    }

    // Add new modal
    document.body.insertAdjacentHTML('beforeend', modalHtml);

    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('dynamicModal'));
    modal.show();
}

// Initial load
document.addEventListener('DOMContentLoaded', function() {
    loadDashboardData();
});
