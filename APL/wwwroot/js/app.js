/**
 * Academic Management System - Full Integration (app.js)
 */

const API_BASE_URL = '/api';

const ApiService = {
    // === DEPARTAMENTE ===
    async getDepartments() {
        const res = await fetch(`${API_BASE_URL}/departments`);
        return await res.json();
    },
    async createDepartment(data) {
        const res = await fetch(`${API_BASE_URL}/departments`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async updateDepartment(id, data) {
        const res = await fetch(`${API_BASE_URL}/departments/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },

    // === PROGRAME DE STUDII ===
    async getStudyPrograms() {
        const res = await fetch(`${API_BASE_URL}/studyprograms`);
        return await res.json();
    },
    async createStudyProgram(data) {
        const res = await fetch(`${API_BASE_URL}/studyprograms`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async updateStudyProgram(id, data) {
        const res = await fetch(`${API_BASE_URL}/studyprograms/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },

    // === DISCIPLINE ===
    async getSubjects() {
        const res = await fetch(`${API_BASE_URL}/subjects`);
        return await res.json();
    },
    async createSubject(data) {
        const res = await fetch(`${API_BASE_URL}/subjects`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async updateSubject(id, data) {
        const res = await fetch(`${API_BASE_URL}/subjects/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },

    // === CURRICULUM ===
    async getCurriculums() {
        const res = await fetch(`${API_BASE_URL}/curriculums`);
        return await res.json();
    },
    async createCurriculum(data) {
        const res = await fetch(`${API_BASE_URL}/curriculums`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async updateCurriculum(id, data) {
        const res = await fetch(`${API_BASE_URL}/curriculums/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async addSubjectToCurriculum(curriculumId, subjectData) {
        const res = await fetch(`${API_BASE_URL}/curriculums/${curriculumId}/subjects`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(subjectData)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },

    // === LIMBI ===
    async getLanguages() {
        const res = await fetch(`${API_BASE_URL}/languages`);
        return await res.json();
    },

    // === PERSOANE & PROFESORI ===
    async searchPersons(query) {
        const res = await fetch(`${API_BASE_URL}/persons/search?q=${encodeURIComponent(query)}`);
        return await res.json();
    },
    async createPersonWithEmployment(data) {
        const res = await fetch(`${API_BASE_URL}/persons`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async updatePerson(id, data) {
        const res = await fetch(`${API_BASE_URL}/persons/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async addEmploymentForExistingPerson(personId, employmentData) {
        const res = await fetch(`${API_BASE_URL}/persons/${personId}/employments`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(employmentData)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async getProfessors() {
        const res = await fetch(`${API_BASE_URL}/professors`);
        return await res.json();
    },

    // === GRUPE & SARCINI ===
    async getGroups() {
        const res = await fetch(`${API_BASE_URL}/groups`);
        return await res.json();
    },
    async createGroup(groupData) {
        const res = await fetch(`${API_BASE_URL}/groups`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(groupData)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async updateGroup(id, groupData) {
        const res = await fetch(`${API_BASE_URL}/groups/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(groupData)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async generateSubgroups(groupId) {
        const res = await fetch(`${API_BASE_URL}/groups/${groupId}/subgroups/generate`, {
            method: 'POST'
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async createTeachingAssignment(assignmentData) {
        const res = await fetch(`${API_BASE_URL}/teachingassignments`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(assignmentData)
        });
        if (!res.ok) throw new Error(await res.text());
        return await res.json();
    },
    async getDashboardStats() {
        const res = await fetch(`${API_BASE_URL}/dashboard/stats`);
        return await res.json();
    }
};

const EmploymentTypeMap = { 'Titular': 0, 'InternalMultiJob': 1, 'ExternalMultiJob': 2 };
const ActivityTypeMap = { 'Lecture': 0, 'Seminar': 1, 'Laboratory': 2 };

document.addEventListener('alpine:init', () => {
    Alpine.data('academicApp', () => ({
        currentTab: 'dashboard',
        activeCurriculumTabId: null,

        tabTitles: {
            'dashboard': 'Dashboard General',
            'departments': 'Gestionare Departamente',
            'studyPrograms': 'Programe de Studii',
            'professors': 'Angajați / Profesori',
            'subjects': 'Gestionare Discipline',
            'curriculum': 'Gestionare Curriculum & Planuri',
            'groups': 'Grupe & Subgrupe',
            'assignments': 'Repartizare Sarcină Didactică',
            'import': 'Import date din Excel',
            'availability': 'Disponibilitate cadre didactice',
            'timetable': 'Orar'
        },

        // Date Nomenclatoare
        stats: {},
        departments: [],
        studyPrograms: [],
        subjects: [],
        professors: [],
        groups: [],
        curriculums: [],
        languages: [],
        searchResults: [],

        // Stări de Editare
        isEditingDepartment: false,
        editingDepartmentId: null,
        isEditingStudyProgram: false,
        editingStudyProgramId: null,
        isEditingPerson: false,
        editingPersonId: null,
        isEditingSubject: false,
        editingSubjectId: null,
        isEditingGroup: false,
        editingGroupId: null,
        // Import Excel
        importFile: null,
        importReport: null,
        importBusy: false,
        importError: '',

        // Orar
        tt: {
            year: '2026-2027', semester: 1, limit: 20, busy: false, error: '', summary: null,
            slots: [], entries: [], rooms: [], viewBy: 'group', viewId: '', roomError: '',
            newRoom: { code: '', building: '', capacity: 30, type: 'Lecture', hasComputers: false }
        },

        // Disponibilitate (doleanțe de timp)
        dayNames: ['Luni', 'Marți', 'Miercuri', 'Joi', 'Vineri', 'Sâmbătă'],
        avail: {
            professorId: '', year: '2026-2027', loaded: false, busy: false, dirty: false, saved: false, error: '',
            slots: [], cells: {}, busy_: {}, maxDays: '', notes: '', commitments: [],
            newC: { day: 1, start: '08:00', end: '10:00', location: '' },
            text: '', parsing: false, parseSource: '', warnings: [], feasibility: []
        },

        isEditingCurriculum: false,
        editingCurriculumId: null,

        // Formulare
        searchQuery: '',
        selectedPerson: null,
        isExistingPerson: false,

        departmentForm: { code: '', name: '', description: '' },
        studyProgramForm: { code: '', name: '', degreeLevel: 'Licență', durationYears: 3 },

        personForm: {
            firstName: '', lastName: '', patronymic: '', email: '', phone: '',
            academicTitle: '', academicDegree: '', departmentId: '',
            employmentType: 'Titular', position: 'Lector', teachingLoad: 1.0
        },

        subjectForm: { code: '', name: '', credits: 5, totalHours: 120, description: '' },

        curriculumForm: {
            studyProgramId: '',
            version: '2026-2027',
            isActive: true
        },

        curriculumSubjectForm: {
            curriculumId: '',
            subjectId: '',
            languageId: '',
            yearOfStudy: 1,
            semester: 1,
            lectureHours: 30,
            seminarHours: 15,
            labHours: 15
        },

        groupForm: {
            code: '',
            studyProgramId: '',
            curriculumId: '',
            yearOfStudy: 1,
            languageId: '',
            studentCount: 20
        },

        assignmentForm: {
            professorProfileId: '',
            subjectId: '',
            groupId: '',
            subgroupId: '',
            languageId: '',
            activityType: 'Lecture',
            hours: 30,
            semester: 1,
            academicYear: '2026-2027'
        },
        assignCheck: null,

        // ===== Orar =====
        ttSlots() { return this.tt.slots.filter(s => !s.isLunchBreak); },
        ttAt(day, slot) { return this.tt.entries.filter(e => e.day === day && e.slot === slot); },
        ttViewOptions() {
            if (this.tt.viewBy === 'group') return this.groups.map(g => ({ id: g.id, label: g.code }));
            if (this.tt.viewBy === 'professor') return this.professors.map(p => ({ id: p.id, label: p.fullName }));
            return this.tt.rooms.map(r => ({ id: r.id, label: r.building + ' · ' + r.code }));
        },

        async loadTimetable() {
            const t = this.tt;
            t.error = '';
            try {
                const [slots, rooms] = await Promise.all([
                    fetch(`${API_BASE_URL}/time-slots`).then(r => r.json()),
                    fetch(`${API_BASE_URL}/rooms`).then(r => r.json())
                ]);
                t.slots = slots; t.rooms = rooms;
                const res = await fetch(`${API_BASE_URL}/timetable/summary?academicYear=${encodeURIComponent(t.year)}&semester=${t.semester}`);
                t.summary = res.ok ? await res.json() : null;
                await this.loadTimetableEntries();
            } catch (e) { t.error = 'Nu s-a putut încărca orarul.'; }
        },

        async loadTimetableEntries() {
            const t = this.tt;
            if (!t.summary) { t.entries = []; return; }
            const key = { group: 'groupId', professor: 'professorId', room: 'roomId' }[t.viewBy];
            const q = `academicYear=${encodeURIComponent(t.year)}&semester=${t.semester}` + (t.viewId ? `&${key}=${t.viewId}` : '');
            const res = await fetch(`${API_BASE_URL}/timetable?${q}`);
            t.entries = res.ok ? await res.json() : [];
        },

        async generateTimetable() {
            const t = this.tt;
            t.busy = true; t.error = '';
            try {
                const res = await fetch(`${API_BASE_URL}/timetable/generate`, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ academicYear: t.year, semester: t.semester, timeLimitSeconds: t.limit })
                });
                const data = await res.json();
                if (!res.ok) { t.error = data.error || 'Generarea a eșuat.'; return; }
                t.summary = data;
                await this.loadTimetableEntries();
            } catch (e) { t.error = 'Nu s-a putut contacta serverul.'; }
            finally { t.busy = false; }
        },

        async addRoom() {
            const t = this.tt; t.roomError = '';
            const res = await fetch(`${API_BASE_URL}/rooms`, {
                method: 'POST', headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ...t.newRoom, capacity: Number(t.newRoom.capacity) })
            });
            const data = await res.json();
            if (!res.ok) { t.roomError = data.error || 'Nu s-a putut adăuga sala.'; return; }
            t.newRoom.code = '';
            t.rooms = await fetch(`${API_BASE_URL}/rooms`).then(r => r.json());
        },

        async toggleRoom(room) {
            await fetch(`${API_BASE_URL}/rooms/${room.id}/active`, {
                method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(!room.active)
            });
            this.tt.rooms = await fetch(`${API_BASE_URL}/rooms`).then(r => r.json());
        },

        async deleteRoom(room) {
            this.tt.roomError = '';
            const res = await fetch(`${API_BASE_URL}/rooms/${room.id}`, { method: 'DELETE' });
            if (!res.ok) { const d = await res.json(); this.tt.roomError = d.error || 'Nu s-a putut șterge sala.'; return; }
            this.tt.rooms = await fetch(`${API_BASE_URL}/rooms`).then(r => r.json());
        },

        // ===== Disponibilitate =====
        availSlots() { return this.avail.slots.filter(s => !s.isLunchBreak); },
        cellKey(d, s) { return d + '-' + s; },
        isBusy(d, s) { return !!this.avail.busy_[this.cellKey(d, s)]; },
        markDirty() { this.avail.dirty = true; this.avail.saved = false; },

        cellClass(d, s) {
            if (this.isBusy(d, s)) return 'bg-slate-300 cursor-not-allowed';
            const st = this.avail.cells[this.cellKey(d, s)];
            if (st === 'Blocked') return 'bg-red-200 text-red-700 hover:bg-red-300';
            if (st === 'Avoid') return 'bg-amber-200 text-amber-700 hover:bg-amber-300';
            if (st === 'Preferred') return 'bg-emerald-200 text-emerald-700 hover:bg-emerald-300';
            return 'bg-white border hover:bg-slate-100 text-slate-300';
        },
        cellIcon(d, s) {
            if (this.isBusy(d, s)) return '⛔';
            return { Blocked: '✕', Avoid: '~', Preferred: '✓' }[this.avail.cells[this.cellKey(d, s)]] || '';
        },
        cycleCell(d, s) {
            if (this.isBusy(d, s)) return;
            const k = this.cellKey(d, s);
            const next = { undefined: 'Blocked', Blocked: 'Avoid', Avoid: 'Preferred', Preferred: undefined }[this.avail.cells[k]];
            const cells = { ...this.avail.cells };
            if (next) cells[k] = next; else delete cells[k];
            this.avail.cells = cells;
            this.markDirty();
        },

        async loadAvailability() {
            const a = this.avail;
            a.error = ''; a.loaded = false; a.warnings = []; a.parseSource = ''; a.feasibility = [];
            if (!a.professorId) return;
            try {
                const res = await fetch(`${API_BASE_URL}/professors/${a.professorId}/availability?academicYear=${encodeURIComponent(a.year)}`);
                const data = await res.json();
                if (!res.ok) { a.error = data.error || 'Nu s-a putut încărca disponibilitatea.'; return; }
                a.slots = data.slots;
                const cells = {};
                data.availability.blocks.forEach(b => cells[this.cellKey(b.day, b.slot)] = b.state);
                a.cells = cells;
                const busy = {};
                data.externallyBusy.forEach(b => busy[this.cellKey(b.day, b.slot)] = true);
                a.busy_ = busy;
                a.maxDays = data.availability.maxTeachingDays ?? '';
                a.notes = data.availability.notes || '';
                a.commitments = data.availability.commitments;
                a.dirty = false; a.saved = false; a.loaded = true;
                await this.loadFeasibility();
            } catch (e) { a.error = 'Nu s-a putut contacta serverul.'; }
        },

        async loadFeasibility() {
            const a = this.avail;
            const res = await fetch(`${API_BASE_URL}/professors/${a.professorId}/availability/feasibility?academicYear=${encodeURIComponent(a.year)}`);
            a.feasibility = res.ok ? await res.json() : [];
        },

        addCommitment() {
            const a = this.avail, c = a.newC;
            if (!c.start || !c.end || c.end <= c.start) { a.error = 'Ora de sfârșit trebuie să fie după ora de început.'; return; }
            a.error = '';
            a.commitments.push({ day: Number(c.day), start: c.start, end: c.end, location: c.location.trim() });
            // celulele acoperite devin „ocupat extern” după salvare; până atunci marchez local
            this.availSlots().forEach(s => {
                if (c.start < s.end && c.end > s.start) this.avail.busy_[this.cellKey(Number(c.day), s.slot)] = true;
            });
            c.location = '';
            this.markDirty();
        },

        async saveAvailability() {
            const a = this.avail;
            a.busy = true; a.error = '';
            try {
                const blocks = Object.entries(a.cells).map(([k, state]) => {
                    const [day, slot] = k.split('-').map(Number);
                    return { day, slot, state };
                });
                const res = await fetch(`${API_BASE_URL}/professors/${a.professorId}/availability`, {
                    method: 'PUT', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        academicYear: a.year, maxTeachingDays: a.maxDays === '' ? null : Number(a.maxDays),
                        notes: a.notes, blocks, commitments: a.commitments
                    })
                });
                const data = await res.json();
                if (!res.ok) { a.error = data.error || 'Salvarea a eșuat.'; return; }
                a.dirty = false; a.saved = true;
                await this.loadAvailability();
                a.saved = true;
            } catch (e) { a.error = 'Nu s-a putut contacta serverul.'; }
            finally { a.busy = false; }
        },

        async parseAvailabilityText() {
            const a = this.avail;
            a.parsing = true; a.error = ''; a.warnings = [];
            try {
                const res = await fetch(`${API_BASE_URL}/professors/${a.professorId}/availability/parse`, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ text: a.text })
                });
                const data = await res.json();
                if (!res.ok) { a.error = data.error || 'Interpretarea a eșuat.'; return; }
                // propunerea înlocuiește marcajele; celulele ocupate extern rămân blocate de angajamente
                const cells = {};
                data.blocks.forEach(b => { if (!this.isBusy(b.day, b.slot)) cells[this.cellKey(b.day, b.slot)] = b.state; });
                a.cells = cells;
                if (data.maxTeachingDays) a.maxDays = data.maxTeachingDays;
                a.warnings = data.warnings;
                a.parseSource = data.source;
                this.markDirty();
            } catch (e) { a.error = 'Nu s-a putut contacta serverul.'; }
            finally { a.parsing = false; }
        },

        onImportFile(event) {
            this.importFile = event.target.files[0] || null;
            this.importReport = null;
            this.importError = '';
            if (this.importFile) this.runImport('preview');
        },

        async runImport(mode) {
            if (!this.importFile) return;
            this.importBusy = true;
            this.importError = '';
            try {
                const form = new FormData();
                form.append('file', this.importFile);
                const res = await fetch(`${API_BASE_URL}/import/${mode}`, { method: 'POST', body: form });
                const data = await res.json();
                if (!res.ok) { this.importError = data.error || 'Importul a eșuat.'; this.importReport = null; return; }
                this.importReport = data;
                if (data.committed) await this.loadAllData();
            } catch (e) {
                this.importError = 'Nu s-a putut contacta serverul.';
            } finally {
                this.importBusy = false;
            }
        },

        resetImport() {
            this.importFile = null;
            this.importReport = null;
            this.importError = '';
            if (this.$refs.importInput) this.$refs.importInput.value = '';
        },

        async init() {
            await this.loadAllData();
        },

        async loadAllData() {
            await Promise.all([
                this.loadDashboard(),
                this.loadDepartments(),
                this.loadStudyPrograms(),
                this.loadProfessors(),
                this.loadSubjects(),
                this.loadCurriculums(),
                this.loadGroups(),
                this.loadLanguages()
            ]);
            if (this.curriculums.length > 0 && !this.activeCurriculumTabId) {
                this.activeCurriculumTabId = this.curriculums[0].id;
            }
        },

        async loadDashboard() { try { this.stats = await ApiService.getDashboardStats(); } catch (e) { console.error(e); } },
        async loadDepartments() { try { this.departments = await ApiService.getDepartments(); } catch (e) { console.error(e); } },
        async loadStudyPrograms() { try { this.studyPrograms = await ApiService.getStudyPrograms(); } catch (e) { console.error(e); } },
        async loadProfessors() { try { this.professors = await ApiService.getProfessors(); } catch (e) { console.error(e); } },
        async loadSubjects() { try { this.subjects = await ApiService.getSubjects(); } catch (e) { console.error(e); } },
        async loadCurriculums() { try { this.curriculums = await ApiService.getCurriculums(); } catch (e) { console.error(e); } },
        async loadGroups() { try { this.groups = await ApiService.getGroups(); } catch (e) { console.error(e); } },
        async loadLanguages() { try { this.languages = await ApiService.getLanguages(); } catch (e) { console.error(e); } },

        // Grouping Curriculums by Group/Year for Tabs
        get curriculumTabs() {
            return this.curriculums.map(c => {
                const sp = this.studyPrograms.find(p => p.id === c.studyProgramId);
                const relatedGroups = this.groups.filter(g => g.curriculumId === c.id);
                const groupCodes = relatedGroups.length > 0 ? relatedGroups.map(g => g.code).join(', ') : (sp ? sp.code : 'N/A');
                return {
                    id: c.id,
                    title: `${groupCodes} — An ${c.version}`,
                    version: c.version,
                    studyProgramName: sp ? sp.name : '',
                    subjects: c.subjects || []
                };
            });
        },

        get filteredCurriculums() {
            if (!this.groupForm.studyProgramId) return this.curriculums;
            return this.curriculums.filter(c => c.studyProgramId === this.groupForm.studyProgramId);
        },

        // === PROGRAME DE STUDII (CREATE / EDIT) ===
        editStudyProgram(sp) {
            this.isEditingStudyProgram = true;
            this.editingStudyProgramId = sp.id;
            this.studyProgramForm = { code: sp.code, name: sp.name, degreeLevel: sp.degreeLevel, durationYears: sp.durationYears };
        },
        cancelStudyProgramEdit() {
            this.isEditingStudyProgram = false;
            this.editingStudyProgramId = null;
            this.studyProgramForm = { code: '', name: '', degreeLevel: 'Licență', durationYears: 3 };
        },
        async submitStudyProgram() {
            try {
                if (!this.studyProgramForm.code || !this.studyProgramForm.name) {
                    alert('Codul și numele programului sunt obligatorii.'); return;
                }
                const payload = {
                    ...this.studyProgramForm,
                    durationYears: parseInt(this.studyProgramForm.durationYears)
                };
                if (this.isEditingStudyProgram) {
                    await ApiService.updateStudyProgram(this.editingStudyProgramId, payload);
                    alert('Programul de studii a fost actualizat!');
                } else {
                    await ApiService.createStudyProgram(payload);
                    alert('Program de studii adăugat!');
                }
                this.cancelStudyProgramEdit();
                await this.loadStudyPrograms();
            } catch (err) { alert('Eroare: ' + err.message); }
        },

        // === DEPARTAMENTE (CREATE / EDIT) ===
        editDepartment(dept) {
            this.isEditingDepartment = true;
            this.editingDepartmentId = dept.id;
            this.departmentForm = { code: dept.code, name: dept.name, description: dept.description || '' };
        },
        cancelDepartmentEdit() {
            this.isEditingDepartment = false;
            this.editingDepartmentId = null;
            this.departmentForm = { code: '', name: '', description: '' };
        },
        async submitDepartment() {
            try {
                if (!this.departmentForm.name || !this.departmentForm.code) {
                    alert('Codul și numele sunt obligatorii.'); return;
                }
                if (this.isEditingDepartment) {
                    await ApiService.updateDepartment(this.editingDepartmentId, this.departmentForm);
                    alert('Departament actualizat!');
                } else {
                    await ApiService.createDepartment(this.departmentForm);
                    alert('Departament adăugat!');
                }
                this.cancelDepartmentEdit();
                await this.loadDepartments();
            } catch (err) { alert('Eroare: ' + err.message); }
        },

        // === PROFESORI / PERSOANE (CREATE / EDIT) ===
        editPerson(prof) {
            this.isEditingPerson = true;
            this.editingPersonId = prof.personId;
            const names = (prof.fullName || '').split(' ');
            this.personForm = {
                firstName: names[1] || '',
                lastName: names[0] || '',
                patronymic: '',
                email: prof.email || '',
                phone: '',
                academicTitle: prof.academicTitle || '',
                academicDegree: '',
                departmentId: (prof.employments && prof.employments[0]) ? prof.employments[0].departmentId : '',
                employmentType: 'Titular',
                position: 'Profesor',
                teachingLoad: 1.0
            };
        },
        cancelPersonEdit() {
            this.isEditingPerson = false;
            this.editingPersonId = null;
            this.resetPersonForm();
        },
        async submitPersonOrEmployment() {
            try {
                if (!this.personForm.departmentId) { alert('Selectați un departament.'); return; }
                const payload = {
                    ...this.personForm,
                    employmentType: EmploymentTypeMap[this.personForm.employmentType] ?? 0,
                    teachingLoad: parseFloat(this.personForm.teachingLoad)
                };

                if (this.isEditingPerson) {
                    await ApiService.updatePerson(this.editingPersonId, payload);
                    alert("Datele profesorului au fost actualizate!");
                    this.cancelPersonEdit();
                } else if (this.isExistingPerson && this.selectedPerson) {
                    await ApiService.addEmploymentForExistingPerson(this.selectedPerson.id, payload);
                    alert("Angajare adăugată!");
                    this.resetPersonForm();
                } else {
                    await ApiService.createPersonWithEmployment(payload);
                    alert("Profesor nou adăugat!");
                    this.resetPersonForm();
                }
                await this.loadProfessors();
                await this.loadDashboard();
            } catch (err) { alert(`Eroare: ${err.message}`); }
        },

        // === DISCIPLINE (CREATE / EDIT) ===
        editSubject(subj) {
            this.isEditingSubject = true;
            this.editingSubjectId = subj.id;
            this.subjectForm = { code: subj.code, name: subj.name, credits: subj.credits, totalHours: subj.totalHours, description: subj.description || '' };
        },
        cancelSubjectEdit() {
            this.isEditingSubject = false;
            this.editingSubjectId = null;
            this.subjectForm = { code: '', name: '', credits: 5, totalHours: 120, description: '' };
        },
        async submitSubject() {
            try {
                if (!this.subjectForm.name || !this.subjectForm.code) {
                    alert('Codul și numele disciplinei sunt obligatorii.'); return;
                }
                const payload = {
                    ...this.subjectForm,
                    credits: parseInt(this.subjectForm.credits),
                    totalHours: parseInt(this.subjectForm.totalHours)
                };
                if (this.isEditingSubject) {
                    await ApiService.updateSubject(this.editingSubjectId, payload);
                    alert('Disciplina a fost actualizată!');
                } else {
                    await ApiService.createSubject(payload);
                    alert('Disciplină adăugată cu succes!');
                }
                this.cancelSubjectEdit();
                await this.loadSubjects();
            } catch (err) { alert('Eroare: ' + err.message); }
        },

        // === CURRICULUM (CREATE / EDIT / ADD SUBJECT) ===
        editCurriculum(curr) {
            this.isEditingCurriculum = true;
            this.editingCurriculumId = curr.id;
            this.curriculumForm = { studyProgramId: curr.studyProgramId, version: curr.version, isActive: curr.isActive };
        },
        cancelCurriculumEdit() {
            this.isEditingCurriculum = false;
            this.editingCurriculumId = null;
            this.curriculumForm = { studyProgramId: '', version: '2026-2027', isActive: true };
        },
        async submitCurriculum() {
            try {
                if (!this.curriculumForm.studyProgramId || !this.curriculumForm.version) {
                    alert('Selectați programul de studii și versiunea.'); return;
                }
                if (this.isEditingCurriculum) {
                    await ApiService.updateCurriculum(this.editingCurriculumId, this.curriculumForm);
                    alert('Curriculum actualizat!');
                } else {
                    await ApiService.createCurriculum(this.curriculumForm);
                    alert('Curriculum adăugat!');
                }
                this.cancelCurriculumEdit();
                await this.loadCurriculums();
            } catch (err) { alert('Eroare: ' + err.message); }
        },
        async submitCurriculumSubject() {
            try {
                if (!this.curriculumSubjectForm.curriculumId || !this.curriculumSubjectForm.subjectId || !this.curriculumSubjectForm.languageId) {
                    alert('Selectați curriculumul, disciplina și limba.'); return;
                }
                await ApiService.addSubjectToCurriculum(this.curriculumSubjectForm.curriculumId, {
                    subjectId: this.curriculumSubjectForm.subjectId,
                    languageId: this.curriculumSubjectForm.languageId,
                    yearOfStudy: parseInt(this.curriculumSubjectForm.yearOfStudy),
                    semester: parseInt(this.curriculumSubjectForm.semester),
                    lectureHours: parseInt(this.curriculumSubjectForm.lectureHours),
                    seminarHours: parseInt(this.curriculumSubjectForm.seminarHours),
                    labHours: parseInt(this.curriculumSubjectForm.labHours)
                });
                alert('Disciplina a fost asociată cu succes în curriculum!');
                await this.loadCurriculums();
                this.curriculumSubjectForm = { curriculumId: '', subjectId: '', languageId: '', yearOfStudy: 1, semester: 1, lectureHours: 30, seminarHours: 15, labHours: 15 };
            } catch (err) { alert('Eroare: ' + err.message); }
        },

        // === GRUPE (CREATE / EDIT) ===
        editGroup(grp) {
            this.isEditingGroup = true;
            this.editingGroupId = grp.id;
            this.groupForm = {
                code: grp.code,
                studyProgramId: grp.studyProgramId,
                curriculumId: grp.curriculumId,
                yearOfStudy: grp.yearOfStudy,
                languageId: grp.languageId,
                studentCount: grp.studentCount
            };
        },
        cancelGroupEdit() {
            this.isEditingGroup = false;
            this.editingGroupId = null;
            this.groupForm = { code: '', studyProgramId: '', curriculumId: '', yearOfStudy: 1, languageId: '', studentCount: 20 };
        },
        async submitGroup() {
            try {
                if (!this.groupForm.code || !this.groupForm.studyProgramId || !this.groupForm.curriculumId || !this.groupForm.languageId) {
                    alert("Completati toate opțiunile din dropdown-uri."); return;
                }

                const payload = {
                    code: this.groupForm.code,
                    studyProgramId: this.groupForm.studyProgramId,
                    curriculumId: this.groupForm.curriculumId,
                    languageId: this.groupForm.languageId,
                    yearOfStudy: parseInt(this.groupForm.yearOfStudy),
                    studentCount: parseInt(this.groupForm.studentCount)
                };

                if (this.isEditingGroup) {
                    await ApiService.updateGroup(this.editingGroupId, payload);
                    if (payload.studentCount > 26) {
                        await ApiService.generateSubgroups(this.editingGroupId);
                    }
                    alert("Grupa a fost actualizată!");
                } else {
                    const createdGroup = await ApiService.createGroup(payload);
                    if (payload.studentCount > 26) {
                        await ApiService.generateSubgroups(createdGroup.id);
                    }
                    alert("Grupa a fost creată cu succes!");
                }
                this.cancelGroupEdit();
                await this.loadGroups();
                await this.loadDashboard();
            } catch (err) { alert(`Eroare la salvarea grupei: ${err.message}`); }
        },

        async submitAssignment() {
            try {
                if (this.assignmentForm.activityType === 'Lecture' && this.assignmentForm.subgroupId) {
                    alert("Cursul nu poate fi atribuit pe subgrupe!"); return;
                }
                const payload = this.assignmentPayload();

                await ApiService.createTeachingAssignment(payload);
                alert("Sarcina didactică a fost salvată!");
                await this.loadDashboard();
                await this.runAssignCheck();
            } catch (err) { alert(`Eroare la repartizare: ${err.message}`); }
        },

        assignmentPayload() {
            const f = this.assignmentForm;
            return {
                professorProfileId: f.professorProfileId,
                subjectId: f.subjectId,
                groupId: f.groupId,
                subgroupId: f.subgroupId || null,
                languageId: f.languageId,
                activityType: ActivityTypeMap[f.activityType] ?? 0,
                hours: parseInt(f.hours),
                semester: parseInt(f.semester),
                academicYear: (f.academicYear || '').trim()
            };
        },

        // Verificare în timp real (nu salvează): reguli + normă anuală + disponibilitate
        async runAssignCheck() {
            const f = this.assignmentForm;
            if (!f.professorProfileId || !f.subjectId || !f.groupId || !f.languageId || !(parseInt(f.hours) > 0)) { this.assignCheck = null; return; }
            try {
                const res = await fetch(`${API_BASE_URL}/teachingassignments/check`, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(this.assignmentPayload())
                });
                this.assignCheck = res.ok ? await res.json() : null;
            } catch (e) { this.assignCheck = null; }
        },

        searchExistingPerson() {
            if (this.searchQuery.length > 2) {
                ApiService.searchPersons(this.searchQuery).then(res => this.searchResults = res);
            } else { this.searchResults = []; }
        },

        selectPersonForEmployment(person) {
            this.selectedPerson = person;
            this.isExistingPerson = true;
            this.searchResults = [];
        },

        resetPersonForm() {
            this.selectedPerson = null;
            this.isExistingPerson = false;
            this.searchQuery = '';
            this.personForm = {
                firstName: '', lastName: '', patronymic: '', email: '', phone: '',
                academicTitle: '', academicDegree: '', departmentId: '',
                employmentType: 'Titular', position: 'Lector', teachingLoad: 1.0
            };
        }
    }));
});