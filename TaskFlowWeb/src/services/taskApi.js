import { apiClient } from './apiClient'

export const taskApi = {
  async getCategories() {
    const res = await apiClient.get('/api/categories')
    return res.data
  },

  async getTasks({ page = 1, pageSize = 10, isCompleted = undefined, categoryId = undefined, searchTitle = undefined, sortBy = undefined } = {}) {
    const params = {
      page,
      pageSize,
    }

    if (typeof isCompleted === 'boolean') params.isCompleted = isCompleted
    if (categoryId !== undefined && categoryId !== '') params.categoryId = Number(categoryId)
    if (searchTitle) params.searchTitle = searchTitle
    if (sortBy) params.sortBy = sortBy

    const res = await apiClient.get('/api/tasks', { params })
    return res.data
  },

  async createTask({ title, description, isCompleted = false, categoryId }) {
    const res = await apiClient.post('/api/tasks', {
      title,
      description,
      isCompleted,
      categoryId: categoryId === '' ? null : categoryId,
    })
    return res.data
  },

  async patchTask({ id, title, description, isCompleted, categoryId }) {
    const res = await apiClient.patch(`/api/tasks/${id}`, {
      title,
      description,
      isCompleted,
      categoryId,
    })
    return res.data
  },

  async deleteTask(id) {
    await apiClient.delete(`/api/tasks/${id}`)
  },
}

