/**
 * API Client for SchedulIX
 * Handles all communication with the backend API using jQuery AJAX
 */
const API_BASE_URL = window.location.origin;

class ScheduleAPIClient {
    constructor() {
        this.baseUrl = `${API_BASE_URL}/api`;
    }

    /**
     * Generic AJAX wrapper returning a Promise
     */
    request(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;

        return new Promise((resolve, reject) => {
            const ajaxSettings = {
                url: url,
                type: options.method || 'GET',
                contentType: 'application/json; charset=utf-8',
                data: options.body || null,
                dataType: options.dataType || 'json',
                success: (data) => resolve(data),
                error: (jqXHR, textStatus, errorThrown) => {
                    const message = jqXHR.responseJSON?.message
                        || jqXHR.responseText
                        || `HTTP ${jqXHR.status}: ${errorThrown}`;

                    console.error('API Error:', message);
                    reject(new Error(message));
                }
            };

            // Custom handling for binary file downloads
            if (options.isBlob) {
                ajaxSettings.xhrFields = { responseType: 'blob' };
                ajaxSettings.dataType = undefined; // Don't parse blob as JSON
                ajaxSettings.success = (data, status, xhr) => {
                    resolve({ blob: data, xhr });
                };
            }

            $.ajax(ajaxSettings);
        });
    }

    // ===== ROOMS ENDPOINTS =====

    /**
     * GET /api/rooms
     * Get all rooms
     */
    async getRooms() {
        return this.request('/rooms');
    }

    /**
     * GET /api/rooms/{id}
     * Get room by ID
     */
    async getRoom(id) {
        return this.request(`/rooms/${id}`);
    }

    /**
     * GET /api/rooms/{id}/calendar
     * Get room calendar
     */
    async getRoomCalendar(id) {
        return this.request(`/rooms/${id}/calendar`);
    }

    /**
     * POST /api/rooms
     * Create new room
     */
    async createRoom(roomData) {
        return this.request('/rooms', {
            method: 'POST',
            body: JSON.stringify(roomData)
        });
    }

    /**
     * PUT /api/rooms/{id}
     * Update room
     */
    async updateRoom(id, roomData) {
        return this.request(`/rooms/${id}`, {
            method: 'PUT',
            body: JSON.stringify(roomData)
        });
    }

    /**
     * PATCH /api/rooms/{id}/status
     * Update room status
     */
    async updateRoomStatus(id, isAvailable) {
        return this.request(`/rooms/${id}/status`, {
            method: 'PATCH',
            body: JSON.stringify({ isAvailable })
        });
    }

    // ===== SCHEDULES ENDPOINTS =====

    /**
     * GET /api/schedules/{id}
     * Get schedule by ID
     */
    async getSchedule(id) {
        return this.request(`/schedules/${id}`);
    }

    /**
     * POST /api/schedules/generate
     * Generate schedule
     */
    async generateSchedule(scheduleRequest) {
        console.log(JSON.stringify(scheduleRequest));
        return this.request('/schedules/generate', {
            method: 'POST',
            body: JSON.stringify(scheduleRequest)
        });
    }

    /**
     * POST /api/schedules/validate
     * Validate schedule
     */
    async validateSchedule(schedule) {
        return this.request('/schedules/validate', {
            method: 'POST',
            body: JSON.stringify(schedule)
        });
    }

    // ===== EXPORT ENDPOINTS =====

    /**
     * GET /api/export/excel
     * Export schedule to Excel
     */
    async exportToExcel() {
        return this.request('/export/excel', { isBlob: true });
    }

    /**
     * GET /api/export/pdf
     * Export schedule to PDF
     */
    async exportToPdf() {
        return this.request('/export/pdf', { isBlob: true });
    }

    /**
     * GET /api/export/room/{roomId}/excel
     * Export room calendar to Excel
     */
    async exportRoomSchedule(roomId) {
        return this.request(`/export/room/${roomId}/excel`, { isBlob: true });
    }
}

// Create global API client instance
const apiClient = new ScheduleAPIClient();

/**
 * Helper function to download files using Blob response
 */
async function downloadFile(apiMethod, filename) {
    try {
        if (typeof showLoading === 'function') showLoading(true);
        const result = await apiMethod();

        if (result && result.blob) {
            // Binary file download via Blob
            const url = window.URL.createObjectURL(result.blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);
        }

        if (typeof showAlert === 'function') {
            showAlert('success', 'Fișierul a fost descărcat cu succes!');
        }
    } catch (error) {
        if (typeof showAlert === 'function') {
            showAlert('danger', `Eroare la descărcare: ${error.message}`);
        }
    } finally {
        if (typeof showLoading === 'function') showLoading(false);
    }
}