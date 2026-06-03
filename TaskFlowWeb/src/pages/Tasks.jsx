import { useEffect, useMemo, useState } from 'react'
import { taskApi } from '../services/taskApi'

function formatDate(iso) {
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

export default function Tasks() {
  const [categories, setCategories] = useState([])
  const [tasks, setTasks] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const [formTitle, setFormTitle] = useState('')
  const [formDescription, setFormDescription] = useState('')
  const [formCategoryId, setFormCategoryId] = useState('')

  const [filter, setFilter] = useState({ isCompleted: '', categoryId: '', searchTitle: '', sortBy: '' })

  const query = useMemo(() => {
    const params = {
      page: 1,
      pageSize: 50,
    }

    if (filter.isCompleted !== '') params.isCompleted = filter.isCompleted === 'true'
    if (filter.categoryId !== '') params.categoryId = Number(filter.categoryId)
    if (filter.searchTitle) params.searchTitle = filter.searchTitle
    if (filter.sortBy) params.sortBy = filter.sortBy

    return params
  }, [filter])

  async function load() {
    setLoading(true)
    setError('')
    try {
      const [catsRes, tasksRes] = await Promise.all([taskApi.getCategories(), taskApi.getTasks(query)])
      setCategories(catsRes)
      setTasks(tasksRes.data || [])
    } catch (err) {
      setError(err?.response?.data?.message || 'Failed to load tasks.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function onCreate(e) {
    e.preventDefault()
    setError('')

    if (!formTitle.trim()) return

    try {
      await taskApi.createTask({
        title: formTitle,
        description: formDescription,
        isCompleted: false,
        categoryId: formCategoryId === '' ? null : Number(formCategoryId),
      })

      setFormTitle('')
      setFormDescription('')
      setFormCategoryId('')
      await load()
    } catch (err) {
      setError(err?.response?.data?.message || 'Failed to create task.')
    }
  }

  async function onToggleComplete(task) {
    try {
      await taskApi.patchTask({
        id: task.id,
        isCompleted: !task.isCompleted,
      })
      await load()
    } catch (err) {
      setError(err?.response?.data?.message || 'Failed to update task.')
    }
  }

  async function onDelete(id) {
    try {
      await taskApi.deleteTask(id)
      await load()
    } catch (err) {
      setError(err?.response?.data?.message || 'Failed to delete task.')
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Your Tasks</h1>
          <p className="mt-1 text-sm text-slate-500">Add tasks, mark them complete, and keep moving.</p>
        </div>

        <div className="w-full md:max-w-sm">
          <div className="grid grid-cols-1 gap-3 rounded-xl border border-slate-200 bg-white p-4 shadow-soft">
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-600">Search</label>
              <input
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-200"
                value={filter.searchTitle}
                onChange={(e) => setFilter((s) => ({ ...s, searchTitle: e.target.value }))}
                placeholder="Title..."
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">Completed</label>
                <select
                  className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-200"
                  value={filter.isCompleted}
                  onChange={(e) => setFilter((s) => ({ ...s, isCompleted: e.target.value }))}
                >
                  <option value="">All</option>
                  <option value="false">No</option>
                  <option value="true">Yes</option>
                </select>
              </div>

              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">Sort</label>
                <select
                  className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-200"
                  value={filter.sortBy}
                  onChange={(e) => setFilter((s) => ({ ...s, sortBy: e.target.value }))}
                >
                  <option value="">Default</option>
                  <option value="title">Title</option>
                  <option value="createdAt">Created</option>
                  <option value="isCompleted">Status</option>
                </select>
              </div>
            </div>

            <div>
              <label className="mb-1 block text-xs font-medium text-slate-600">Category</label>
              <select
                className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-200"
                value={filter.categoryId}
                onChange={(e) => setFilter((s) => ({ ...s, categoryId: e.target.value }))}
              >
                <option value="">All</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="flex gap-2">
              <button
                className="flex-1 rounded-md bg-brand-600 py-2 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-60"
                onClick={load}
                disabled={loading}
              >
                {loading ? 'Loading...' : 'Apply'}
              </button>
              <button
                className="rounded-md border border-slate-300 bg-white px-3 py-2 text-sm font-semibold text-slate-800 hover:bg-slate-50"
                onClick={() => setFilter({ isCompleted: '', categoryId: '', searchTitle: '', sortBy: '' })}
                disabled={loading}
                type="button"
              >
                Reset
              </button>
            </div>
          </div>
        </div>
      </div>

      {error ? (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      ) : null}

      <div className="grid gap-6 md:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-soft">
          <h2 className="mb-3 text-lg font-semibold">Add Task</h2>
          <form onSubmit={onCreate} className="space-y-3">
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-600">Title</label>
              <input
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-200"
                value={formTitle}
                onChange={(e) => setFormTitle(e.target.value)}
                placeholder="e.g. Study API"
                required
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-600">Description</label>
              <textarea
                className="min-h-[90px] w-full resize-none rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-200"
                value={formDescription}
                onChange={(e) => setFormDescription(e.target.value)}
                placeholder="What do you need to do?"
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-600">Category</label>
              <select
                className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-200"
                value={formCategoryId}
                onChange={(e) => setFormCategoryId(e.target.value)}
              >
                <option value="">None</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
            </div>

            <button
              className="w-full rounded-md bg-brand-600 py-2.5 text-sm font-semibold text-white hover:bg-brand-700"
              type="submit"
              disabled={loading}
            >
              Create task
            </button>
          </form>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-soft">
          <h2 className="mb-3 text-lg font-semibold">Task List</h2>

          {tasks.length === 0 ? (
            <div className="rounded-lg border border-dashed border-slate-300 p-4 text-sm text-slate-600">
              No tasks found.
            </div>
          ) : (
            <ul className="space-y-3">
              {tasks.map((t) => (
                <li key={t.id} className="rounded-lg border border-slate-200 bg-slate-50 p-3">
                  <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <input
                          type="checkbox"
                          checked={t.isCompleted}
                          onChange={() => onToggleComplete(t)}
                          className="h-4 w-4 accent-brand-600"
                        />
                        <div className="truncate text-sm font-semibold">
                          {t.title}{' '}
                          {t.categoryName ? (
                            <span className="ml-2 rounded-full bg-brand-50 px-2 py-0.5 text-xs font-medium text-brand-700 border border-brand-200">
                              {t.categoryName}
                            </span>
                          ) : null}
                        </div>
                      </div>

                      {t.description ? (
                        <p className="mt-1 line-clamp-2 text-sm text-slate-600">{t.description}</p>
                      ) : null}

                      <div className="mt-2 text-xs text-slate-500">
                        Created: {formatDate(t.createdAt)}
                      </div>
                    </div>

                    <button
                      type="button"
                      onClick={() => onDelete(t.id)}
                      className="rounded-md border border-red-200 bg-white px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50"
                      aria-label={`Delete ${t.title}`}
                    >
                      Delete
                    </button>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}

