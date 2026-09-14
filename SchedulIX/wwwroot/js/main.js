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
    } else if (pageId === 'students') {
        loadStudents();
    } else if (pageId === 'groups') {
        loadGroups();
    } else if (pageId === 'teachers') {
        loadTeachers();
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

// Caches for client-side comboboxes
let groupsCache = [];
let educationFormsCache = [];

async function loadGroupsCache() {
    try {
        groupsCache = Array.isArray(await apiClient.getGroups()) ? await apiClient.getGroups() : [];
    } catch (err) {
        groupsCache = [];
        console.warn('Failed to load groups cache', err.message);
    }
}

async function loadEducationForms() {
    try {
        educationFormsCache = Array.isArray(await apiClient.getEducationForms()) ? await apiClient.getEducationForms() : [];

        // populate addGroup select if present
        const addSelect = document.getElementById('groupAddEducationFormId');
        if (addSelect) {
            addSelect.innerHTML = '<option value="">Selectează Forma Educațională...</option>' +
                educationFormsCache.map(e => `<option value="${e.id}">${escapeHtml(e.name)} </option>`).join('');
        }

        // populate modal select if present
        const modalSelect = document.getElementById('groupEducationFormId');
        if (modalSelect) {
            modalSelect.innerHTML = '<option value="">Selectează Forma Educațională...</option>' +
                educationFormsCache.map(e => `<option value="${e.id}">${escapeHtml(e.name)} </option>`).join('');
        }
    } catch (err) {
        educationFormsCache = [];
        console.warn('Failed to load education forms', err.message);
    }
}

function setupStudentGroupCombobox() {
    const input = document.getElementById('studentAddGroupSearch');
    const hidden = document.getElementById('studentAddGroupId');
    const suggestions = document.getElementById('studentGroupSuggestions');
    if (!input || !hidden || !suggestions) return;

    input.addEventListener('input', () => {
        const q = input.value.trim().toLowerCase();
        if (!q) {
            suggestions.style.display = 'none';
            suggestions.innerHTML = '';
            hidden.value = '';
            return;
        }

        const matches = groupsCache.filter(g => (g.name || '').toLowerCase().includes(q)).slice(0, 10);
        if (matches.length === 0) {
            suggestions.style.display = 'none';
            suggestions.innerHTML = '';
            hidden.value = '';
            return;
        }

        suggestions.innerHTML = matches.map(g => `
            <button type="button" class="list-group-item list-group-item-action" data-id="${g.id}">${escapeHtml(g.name)}</button>
        `).join('');
        suggestions.style.display = 'block';

        suggestions.querySelectorAll('button').forEach(btn => {
            btn.addEventListener('click', () => {
                const id = btn.getAttribute('data-id');
                const name = btn.textContent;
                hidden.value = id;
                input.value = name;
                suggestions.style.display = 'none';
            });
        });
    });

    // hide on outside click
    document.addEventListener('click', (e) => {
        if (!input.contains(e.target) && !suggestions.contains(e.target)) {
            suggestions.style.display = 'none';
        }
    });
}


async function loadDashboardData() {
    try {
        showLoading(true);

        // Load rooms count
        const rooms = await apiClient.getRooms();
        const roomCount = Array.isArray(rooms) ? rooms.length : 0;
        document.getElementById('roomCount').textContent = roomCount;

        // Load students, groups and teachers counts (if endpoints exist)
        try {
            const students = await apiClient.getStudents();
            const studentCount = Array.isArray(students) ? students.length : 0;
            const el = document.getElementById('studentCount');
            if (el) el.textContent = studentCount;
        } catch (err) {
            // silently ignore if endpoint not present
            console.warn('getStudents failed', err.message);
        }

        try {
            const groups = await apiClient.getGroups();
            const groupCount = Array.isArray(groups) ? groups.length : 0;
            const el = document.getElementById('groupCount');
            if (el) el.textContent = groupCount;
        } catch (err) {
            console.warn('getGroups failed', err.message);
        }

        try {
            const teachers = await apiClient.getTeachers();
            const teacherCount = Array.isArray(teachers) ? teachers.length : 0;
            const el = document.getElementById('teacherCount');
            if (el) el.textContent = teacherCount;
        } catch (err) {
            console.warn('getTeachers failed', err.message);
        }

        showAlert('info', 'Dashboard actualizat cu succes!');
    } catch (error) {
        showAlert('danger', `Eroare la încărcare dashboard: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

// ===== PAGE: STUDENTS / GROUPS / TEACHERS =====

async function loadStudents() {
    try {
        showLoading(true);
        const students = await apiClient.getStudents();
        const tableBody = document.getElementById('studentsTableBody');
        const countEl = document.getElementById('studentCount');

        const count = Array.isArray(students) ? students.length : 0;
        if (countEl) countEl.textContent = count;

        if (!Array.isArray(students) || students.length === 0) {
            tableBody.innerHTML = `
                <tr><td colspan="4" class="text-center text-muted">Nu sunt studenți în baza de date</td></tr>
            `;
            return;
        }

        tableBody.innerHTML = students.map(s => `
            <tr>
                <td>${escapeHtml(String(s.id))}</td>
                <td>${escapeHtml(s.firstName || '')}</td>
                <td>${escapeHtml(s.lastName || '')}</td>
                <td>
                    <button class="btn btn-sm btn-primary me-1" onclick="showStudentForm(${s.id})">Editează</button>
                    <button class="btn btn-sm btn-danger" onclick="deleteStudent(${s.id})">Șterge</button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        showAlert('danger', `Eroare la încărcare studenți: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

/**
 * Show student form for create or edit. If id is provided, loads student data.
 */
async function showStudentForm(id) {
    try {
        showLoading(true);
        // ensure groups cache available for select options
        await loadGroupsCache();

        let student = { id: null, firstName: '', lastName: '', email: '', groupId: '', subgroupId: '' };
        if (id) {
            student = await apiClient.getStudent(id);
        }
        const groupOptions = groupsCache.map(g => `<option value="${g.id}" ${g.id === student?.groupId ? 'selected' : ''}>${escapeHtml(g.name)}</option>`).join('');

        const html = `
            <form id="studentForm">
                <input type="hidden" id="studentId" value="${student?.id ?? ''}" />
                <div class="mb-3">
                    <label class="form-label">Prenume</label>
                    <input class="form-control" id="studentFirstName" value="${escapeHtml(student?.firstName ?? '')}" required />
                </div>
                <div class="mb-3">
                    <label class="form-label">Nume</label>
                    <input class="form-control" id="studentLastName" value="${escapeHtml(student?.lastName ?? '')}" required />
                </div>
                <div class="mb-3">
                    <label class="form-label">Email</label>
                    <input type="email" class="form-control" id="studentEmail" value="${escapeHtml(student?.email ?? '')}" required />
                </div>
                <div class="mb-3">
                    <label class="form-label">Grupă</label>
                    <select id="studentGroupId" class="form-select" required>
                        <option value="">Selectează grupă...</option>
                        ${groupOptions}
                    </select>
                </div>
                <div class="mb-3">
                    <label class="form-label">SubgroupId</label>
                    <input type="number" class="form-control" id="studentSubgroupId" value="${student?.subgroupId ?? ''}" />
                </div>
                <div class="text-end">
                    <button type="submit" class="btn btn-primary">Salvează</button>
                    <button type="button" class="btn btn-secondary ms-2" data-bs-dismiss="modal">Anulează</button>
                </div>
            </form>
        `;

        showModal(id ? 'Editează Student' : 'Adaugă Student', html);

        // attach submit handler
        const form = document.getElementById('studentForm');
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            await submitStudentForm();
        });


    } catch (err) {
        showAlert('danger', `Eroare: ${err.message}`);
    } finally {
        showLoading(false);
    }
}

async function submitStudentForm() {
    try {
        showLoading(true);
        const id = document.getElementById('studentId').value;
        const payload = {
            firstName: document.getElementById('studentFirstName').value.trim(),
            lastName: document.getElementById('studentLastName').value.trim(),
            email: document.getElementById('studentEmail').value.trim(),
            groupId: parseInt(document.getElementById('studentGroupId').value, 10),
            subgroupId: document.getElementById('studentSubgroupId').value ? parseInt(document.getElementById('studentSubgroupId').value, 10) : null
        };

        if (id) {
            await apiClient.updateStudent(id, payload);
            showAlert('success', 'Student actualizat cu succes');
        } else {
            await apiClient.createStudent(payload);
            showAlert('success', 'Student creat cu succes');
        }

        // close modal
        const modalEl = document.getElementById('dynamicModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        modal.hide();

        await loadStudents();
    } catch (err) {
        showAlert('danger', `Eroare la salvare student: ${err.message}`);
    } finally {
        showLoading(false);
    }
}

async function deleteStudent(id) {
    if (!confirm('Sigur doriți să ștergeți acest student?')) return;
    try {
        showLoading(true);
        await apiClient.deleteStudent(id);
        showAlert('success', 'Student șters cu succes');
        await loadStudents();
    } catch (err) {
        showAlert('danger', `Eroare la ștergere: ${err.message}`);
    } finally {
        showLoading(false);
    }
}

async function loadGroups() {
    try {
        showLoading(true);
        const groups = await apiClient.getGroups();
        const tableBody = document.getElementById('groupsTableBody');
        const countEl = document.getElementById('groupCount');

        const count = Array.isArray(groups) ? groups.length : 0;
        if (countEl) countEl.textContent = count;

        if (!Array.isArray(groups) || groups.length === 0) {
            tableBody.innerHTML = `
                <tr><td colspan="3" class="text-center text-muted">Nu sunt grupe în baza de date</td></tr>
            `;
            return;
        }

        tableBody.innerHTML = groups.map(g => `
            <tr>
                <td>${escapeHtml(String(g.id))}</td>
                <td>${escapeHtml(g.name || '')}</td>
                <td>
                    <button class="btn btn-sm btn-primary me-1" onclick="showGroupForm(${g.id})">Editează</button>
                    <button class="btn btn-sm btn-danger" onclick="deleteGroup(${g.id})">Șterge</button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        showAlert('danger', `Eroare la încărcare grupe: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

// Group form/modal handlers
async function showGroupForm(id) {
    try {
        showLoading(true);
        // ensure education forms are loaded for the select
        await loadEducationForms();
        let group = { id: null, name: '', educationFormId: '' };
        if (id) {
            group = await apiClient.getGroup(id);
        }
        const options = educationFormsCache.map(e => `<option value="${e.id}" ${e.id === group?.educationFormId ? 'selected' : ''}>${escapeHtml(e.name)} (${e.year}/${e.semester})</option>`).join('');

        const html = `
            <form id="groupForm">
                <input type="hidden" id="groupId" value="${group?.id ?? ''}" />
                <div class="mb-3">
                    <label class="form-label">Nume Grupă</label>
                    <input class="form-control" id="groupName" value="${escapeHtml(group?.name ?? '')}" required />
                </div>
                <div class="mb-3">
                    <label class="form-label">Forma Educațională</label>
                    <select id="groupEducationFormId" class="form-select" required>
                        <option value="">Selectează Forma Educațională...</option>
                        ${options}
                    </select>
                </div>
                <div class="text-end">
                    <button type="submit" class="btn btn-primary">Salvează</button>
                    <button type="button" class="btn btn-secondary ms-2" data-bs-dismiss="modal">Anulează</button>
                </div>
            </form>
        `;

        showModal(id ? 'Editează Grupă' : 'Adaugă Grupă', html);

        const form = document.getElementById('groupForm');
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            await submitGroupForm();
        });
    } catch (err) {
        showAlert('danger', `Eroare: ${err.message}`);
    } finally {
        showLoading(false);
    }
}

async function submitGroupForm() {
    try {
        showLoading(true);
        const id = document.getElementById('groupId').value;
        const payload = {
            name: document.getElementById('groupName').value.trim(),
            educationFormId: parseInt(document.getElementById('groupEducationFormId').value, 10)
        };

        if (id) {
            await apiClient.updateGroup(id, payload);
            showAlert('success', 'Grupa actualizată cu succes');
        } else {
            await apiClient.createGroup(payload);
            showAlert('success', 'Grupa creată cu succes');
        }

        const modalEl = document.getElementById('dynamicModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        modal.hide();

        await loadGroups();
    } catch (err) {
        showAlert('danger', `Eroare la salvare grupă: ${err.message}`);
    } finally {
        showLoading(false);
    }
}

async function deleteGroup(id) {
    if (!confirm('Sigur doriți să ștergeți această grupă?')) return;
    try {
        showLoading(true);
        await apiClient.deleteGroup(id);
        showAlert('success', 'Grupa ștearsă cu succes');
        await loadGroups();
    } catch (err) {
        showAlert('danger', `Eroare la ștergere: ${err.message}`);
    } finally {
        showLoading(false);
    }
}

async function loadTeachers() {
    try {
        showLoading(true);
        const teachers = await apiClient.getTeachers();
        const tableBody = document.getElementById('teachersTableBody');
        const countEl = document.getElementById('teacherCount');

        const count = Array.isArray(teachers) ? teachers.length : 0;
        if (countEl) countEl.textContent = count;

        if (!Array.isArray(teachers) || teachers.length === 0) {
            tableBody.innerHTML = `
                <tr><td colspan="3" class="text-center text-muted">Nu sunt profesori în baza de date</td></tr>
            `;
            return;
        }

        tableBody.innerHTML = teachers.map(t => `
            <tr>
                <td>${escapeHtml(String(t.id))}</td>
                <td>${escapeHtml((t.firstName || '') + ' ' + (t.lastName || ''))}</td>
                <td>
                    <button class="btn btn-sm btn-primary me-1" onclick="showTeacherForm(${t.id})">Editează</button>
                    <button class="btn btn-sm btn-danger" onclick="deleteTeacher(${t.id})">Șterge</button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        showAlert('danger', `Eroare la încărcare profesori: ${error.message}`);
    } finally {
        showLoading(false);
    }
}

// Teacher form/modal handlers
async function showTeacherForm(id) {
    try {
        showLoading(true);
        let teacher = { id: null, firstName: '', lastName: '', email: '' };
        if (id) {
            teacher = await apiClient.getTeacher(id);
        }

        const html = `
            <form id="teacherForm">
                <input type="hidden" id="teacherId" value="${teacher?.id ?? ''}" />
                <div class="mb-3">
                    <label class="form-label">Prenume</label>
                    <input class="form-control" id="teacherFirstName" value="${escapeHtml(teacher?.firstName ?? '')}" required />
                </div>
                <div class="mb-3">
                    <label class="form-label">Nume</label>
                    <input class="form-control" id="teacherLastName" value="${escapeHtml(teacher?.lastName ?? '')}" required />
                </div>
                <div class="mb-3">
                    <label class="form-label">Email</label>
                    <input type="email" class="form-control" id="teacherEmail" value="${escapeHtml(teacher?.email ?? '')}" required />
                </div>
                <div class="text-end">
                    <button type="submit" class="btn btn-primary">Salvează</button>
                    <button type="button" class="btn btn-secondary ms-2" data-bs-dismiss="modal">Anulează</button>
                </div>
            </form>
        `;

        showModal(id ? 'Editează Profesor' : 'Adaugă Profesor', html);

        const form = document.getElementById('teacherForm');
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            await submitTeacherForm();
        });
    } catch (err) {
        showAlert('danger', `Eroare: ${err.message}`);
    } finally {
        showLoading(false);
    }
}

async function submitTeacherForm() {
    try {
        showLoading(true);
        const id = document.getElementById('teacherId').value;
        const payload = {
            firstName: document.getElementById('teacherFirstName').value.trim(),
            lastName: document.getElementById('teacherLastName').value.trim(),
            email: document.getElementById('teacherEmail').value.trim()
        };

        if (id) {
            await apiClient.updateTeacher(id, payload);
            showAlert('success', 'Profesor actualizat cu succes');
        } else {
            await apiClient.createTeacher(payload);
            showAlert('success', 'Profesor creat cu succes');
        }

        const modalEl = document.getElementById('dynamicModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        modal.hide();

        await loadTeachers();
    } catch (err) {
        showAlert('danger', `Eroare la salvare profesor: ${err.message}`);
    } finally {
        showLoading(false);
    }
}

async function deleteTeacher(id) {
    if (!confirm('Sigur doriți să ștergeți acest profesor?')) return;
    try {
        showLoading(true);
        await apiClient.deleteTeacher(id);
        showAlert('success', 'Profesor șters cu succes');
        await loadTeachers();
    } catch (err) {
        showAlert('danger', `Eroare la ștergere: ${err.message}`);
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
    // Add Student Form
    const addStudentForm = document.getElementById('addStudentForm');
    if (addStudentForm) {
        addStudentForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            try {
                showLoading(true);
                const groupHidden = document.getElementById('studentAddGroupId');
                const groupSearch = document.getElementById('studentAddGroupSearch');
                let groupId = groupHidden && groupHidden.value ? parseInt(groupHidden.value, 10) : null;

                // fallback: if user typed but didn't select, try to find a matching group by substring
                if (!groupId && groupSearch && groupSearch.value) {
                    const q = groupSearch.value.trim().toLowerCase();
                    const found = groupsCache.find(g => (g.name || '').toLowerCase().includes(q));
                    if (found) groupId = found.id;
                }

                const payload = {
                    firstName: document.getElementById('studentAddFirstName').value.trim(),
                    lastName: document.getElementById('studentAddLastName').value.trim(),
                    email: document.getElementById('studentAddEmail').value.trim(),
                    groupId: groupId
                };

                await apiClient.createStudent(payload);
                showAlert('success', 'Studentul a fost adăugat cu succes!');
                addStudentForm.reset();
                await loadStudents();
            } catch (error) {
                showAlert('danger', `Eroare la adăugare student: ${error.message}`);
            } finally {
                showLoading(false);
            }
        });
    }

    // Add Group Form
    const addGroupForm = document.getElementById('addGroupForm');
    if (addGroupForm) {
        addGroupForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            try {
                showLoading(true);
                const payload = {
                    name: document.getElementById('groupAddName').value.trim(),
                    educationFormId: parseInt(document.getElementById('groupAddEducationFormId').value, 10)
                };

                await apiClient.createGroup(payload);
                showAlert('success', 'Grupa a fost adăugată cu succes!');
                addGroupForm.reset();
                await loadGroups();
            } catch (error) {
                showAlert('danger', `Eroare la adăugare grupă: ${error.message}`);
            } finally {
                showLoading(false);
            }
        });
    }

    // Add Teacher Form
    const addTeacherForm = document.getElementById('addTeacherForm');
    if (addTeacherForm) {
        addTeacherForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            try {
                showLoading(true);
                const payload = {
                    firstName: document.getElementById('teacherAddFirstName').value.trim(),
                    lastName: document.getElementById('teacherAddLastName').value.trim(),
                    email: document.getElementById('teacherAddEmail').value.trim()
                };

                await apiClient.createTeacher(payload);
                showAlert('success', 'Profesorul a fost adăugat cu succes!');
                addTeacherForm.reset();
                await loadTeachers();
            } catch (error) {
                showAlert('danger', `Eroare la adăugare profesor: ${error.message}`);
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
    if (text === null || text === undefined) return '';

    // Ensure text is converted to a string (handles numbers, booleans, etc.)
    const str = String(text);

    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return str.replace(/[&<>"']/g, m => map[m]);
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

// Load caches and setup comboboxes after DOM ready
document.addEventListener('DOMContentLoaded', function() {
    // populate education forms and groups cache for selects and combobox
    loadEducationForms();
    loadGroupsCache().then(() => {
        setupStudentGroupCombobox();
    });
});

// Expose functions to global scope so inline onclick handlers and HTML attributes can call them
// (Some environments or bundlers may wrap files and prevent implicit globals.)
window.showPage = showPage;
window.loadStudents = loadStudents;
window.loadGroups = loadGroups;
window.loadTeachers = loadTeachers;
window.loadRooms = loadRooms;
window.loadSchedules = typeof loadSchedules === 'function' ? loadSchedules : undefined;
window.showStudentForm = showStudentForm;
window.deleteStudent = deleteStudent;
window.showGroupForm = showGroupForm;
window.submitGroupForm = submitGroupForm;
window.deleteGroup = deleteGroup;
window.showTeacherForm = showTeacherForm;
window.submitTeacherForm = submitTeacherForm;
window.deleteTeacher = deleteTeacher;
