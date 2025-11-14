import { useEffect, useState } from 'react'
import {
  Container,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Box,
  IconButton,
  Chip,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import api from '../services/api'
import { Department, Role } from '../types'

export default function Departments() {
  const [departments, setDepartments] = useState<Department[]>([])
  const [roles, setRoles] = useState<{ [key: number]: Role[] }>({})
  const [open, setOpen] = useState(false)
  const [roleOpen, setRoleOpen] = useState<number | null>(null)
  const [formData, setFormData] = useState({
    name: '',
    code: '',
    description: '',
  })
  const [roleData, setRoleData] = useState({ name: '', description: '' })

  useEffect(() => {
    fetchDepartments()
  }, [])

  const fetchDepartments = async () => {
    try {
      const response = await api.get('/departments')
      setDepartments(response.data)
      // Fetch roles for each department
      for (const dept of response.data) {
        fetchRoles(dept.id)
      }
    } catch (error) {
      console.error('Error fetching departments:', error)
    }
  }

  const fetchRoles = async (departmentId: number) => {
    try {
      const response = await api.get(`/departments/${departmentId}/roles`)
      setRoles((prev) => ({ ...prev, [departmentId]: response.data }))
    } catch (error) {
      console.error('Error fetching roles:', error)
    }
  }

  const handleCreate = async () => {
    try {
      await api.post('/departments', formData)
      setOpen(false)
      setFormData({ name: '', code: '', description: '' })
      fetchDepartments()
    } catch (error: any) {
      alert(error.response?.data?.message || 'Lỗi khi tạo phòng ban')
    }
  }

  const handleCreateRole = async (departmentId: number) => {
    try {
      await api.post(`/departments/${departmentId}/roles`, roleData)
      setRoleOpen(null)
      setRoleData({ name: '', description: '' })
      fetchRoles(departmentId)
    } catch (error: any) {
      alert(error.response?.data?.message || 'Lỗi khi tạo chức vụ')
    }
  }

  const handleDelete = async (id: number) => {
    if (!confirm('Bạn có chắc muốn xóa phòng ban này?')) return
    try {
      await api.delete(`/departments/${id}`)
      fetchDepartments()
    } catch (error) {
      alert('Lỗi khi xóa phòng ban')
    }
  }

  return (
    <Container>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4">Quản lý phòng ban</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setOpen(true)}
        >
          Tạo phòng ban
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Tên phòng ban</TableCell>
              <TableCell>Mã</TableCell>
              <TableCell>Mô tả</TableCell>
              <TableCell>Trạng thái</TableCell>
              <TableCell>Thao tác</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {departments.map((dept) => (
              <TableRow key={dept.id}>
                <TableCell>{dept.id}</TableCell>
                <TableCell>{dept.name}</TableCell>
                <TableCell>{dept.code || 'N/A'}</TableCell>
                <TableCell>{dept.description || 'N/A'}</TableCell>
                <TableCell>
                  <Chip
                    label={dept.isActive ? 'Hoạt động' : 'Không hoạt động'}
                    color={dept.isActive ? 'success' : 'default'}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <IconButton
                    color="error"
                    onClick={() => handleDelete(dept.id)}
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Box sx={{ mt: 4 }}>
        <Typography variant="h5" gutterBottom>
          Chức vụ theo phòng ban
        </Typography>
        {departments.map((dept) => (
          <Accordion key={dept.id}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography>
                {dept.name} ({roles[dept.id]?.length || 0} chức vụ)
              </Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Box sx={{ mb: 2 }}>
                <Button
                  size="small"
                  startIcon={<AddIcon />}
                  onClick={() => setRoleOpen(dept.id)}
                >
                  Thêm chức vụ
                </Button>
              </Box>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>ID</TableCell>
                    <TableCell>Tên chức vụ</TableCell>
                    <TableCell>Mô tả</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {roles[dept.id]?.map((role) => (
                    <TableRow key={role.id}>
                      <TableCell>{role.id}</TableCell>
                      <TableCell>{role.name}</TableCell>
                      <TableCell>{role.description || 'N/A'}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </AccordionDetails>
          </Accordion>
        ))}
      </Box>

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Tạo phòng ban mới</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              label="Tên phòng ban"
              required
              value={formData.name}
              onChange={(e) =>
                setFormData({ ...formData, name: e.target.value })
              }
            />
            <TextField
              label="Mã"
              value={formData.code}
              onChange={(e) =>
                setFormData({ ...formData, code: e.target.value })
              }
            />
            <TextField
              label="Mô tả"
              multiline
              rows={3}
              value={formData.description}
              onChange={(e) =>
                setFormData({ ...formData, description: e.target.value })
              }
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Hủy</Button>
          <Button
            onClick={handleCreate}
            variant="contained"
            disabled={!formData.name}
          >
            Tạo
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={roleOpen !== null}
        onClose={() => setRoleOpen(null)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Thêm chức vụ</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              label="Tên chức vụ"
              required
              value={roleData.name}
              onChange={(e) =>
                setRoleData({ ...roleData, name: e.target.value })
              }
            />
            <TextField
              label="Mô tả"
              multiline
              rows={3}
              value={roleData.description}
              onChange={(e) =>
                setRoleData({ ...roleData, description: e.target.value })
              }
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRoleOpen(null)}>Hủy</Button>
          <Button
            onClick={() => roleOpen && handleCreateRole(roleOpen)}
            variant="contained"
            disabled={!roleData.name}
          >
            Tạo
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}

