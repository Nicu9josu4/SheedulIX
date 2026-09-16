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
            'assignments': 'Repartizare Sarcină Didactică'
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
            semester: 1
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
                const payload = {
                    professorProfileId: this.assignmentForm.professorProfileId,
                    subjectId: this.assignmentForm.subjectId,
                    groupId: this.assignmentForm.groupId,
                    subgroupId: this.assignmentForm.subgroupId || null,
                    languageId: this.assignmentForm.languageId,
                    activityType: ActivityTypeMap[this.assignmentForm.activityType] ?? 0,
                    hours: parseInt(this.assignmentForm.hours),
                    semester: parseInt(this.assignmentForm.semester)
                };

                await ApiService.createTeachingAssignment(payload);
                alert("Sarcina didactică a fost salvată!");
                await this.loadDashboard();
            } catch (err) { alert(`Eroare la repartizare: ${err.message}`); }
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