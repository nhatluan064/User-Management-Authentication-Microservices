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
  MenuItem,
  Chip,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import CancelIcon from '@mui/icons-material/Cancel'
import DownloadIcon from '@mui/icons-material/Download'
import api from '../services/api'
import { DelegationInfo, CreateDelegationRequest, UserInfo } from '../types'

export default function Delegations() {
  const [delegations, setDelegations] = useState<DelegationInfo[]>([])
  const [users, setUsers] = useState<UserInfo[]>([])
  const [open, setOpen] = useState(false)
  const [formData, setFormData] = useState<CreateDelegationRequest>({
    delegateeId: 0,
    startDate: '',
    endDate: '',
    reason: '',
  })

  useEffect(() => {
    fetchDelegations()
    fetchUsers()
  }, [])

  const fetchDelegations = async () => {
    try {
      const response = await api.get('/delegations/my-delegations')
      setDelegations(response.data)
    } catch (error) {
      console.error('Error fetching delegations:', error)
    }
  }

  const fetchUsers = async () => {
    try {
      const response = await api.get('/users')
      setUsers(response.data)
    } catch (error) {
      console.error('Error fetching users:', error)
    }
  }

  const handleCreate = async () => {
    try {
      await api.post('/delegations', formData)
      setOpen(false)
      setFormData({
        delegateeId: 0,
        startDate: '',
        endDate: '',
        reason: '',
      })
      fetchDelegations()
    } catch (error: any) {
      alert(error.response?.data?.message || 'Lỗi khi tạo ủy quyền')
    }
  }

  const handleCancel = async (id: number) => {
    if (!confirm('Bạn có chắc muốn hủy ủy quyền này?')) return
    try {
      await api.post(`/delegations/${id}/cancel`)
      fetchDelegations()
    } catch (error) {
      alert('Lỗi khi hủy ủy quyền')
    }
  }

  const handleDownloadPdf = async (id: number) => {
    try {
      const response = await api.get(`/delegations/${id}/pdf`, {
        responseType: 'blob',
      })
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', `delegation_${id}.pdf`)
      document.body.appendChild(link)
      link.click()
      link.remove()
    } catch (error) {
      alert('Lỗi khi tải PDF')
    }
  }

  const isActive = (delegation: DelegationInfo) => {
    const now = new Date()
    const start = new Date(delegation.startDate)
    const end = new Date(delegation.endDate)
    return now >= start && now <= end
  }

  return (
    <Container>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4">Quản lý ủy quyền</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setOpen(true)}
        >
          Tạo ủy quyền
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Người được ủy quyền</TableCell>
              <TableCell>Từ ngày</TableCell>
              <TableCell>Đến ngày</TableCell>
              <TableCell>Lý do</TableCell>
              <TableCell>Trạng thái</TableCell>
              <TableCell>Thao tác</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {delegations.map((delegation) => (
              <TableRow key={delegation.delegationId}>
                <TableCell>{delegation.delegationId}</TableCell>
                <TableCell>
                  {delegation.delegator.fullName || delegation.delegator.username}
                </TableCell>
                <TableCell>
                  {new Date(delegation.startDate).toLocaleDateString('vi-VN')}
                </TableCell>
                <TableCell>
                  {new Date(delegation.endDate).toLocaleDateString('vi-VN')}
                </TableCell>
                <TableCell>{delegation.reason || 'N/A'}</TableCell>
                <TableCell>
                  <Chip
                    label={isActive(delegation) ? 'Đang hoạt động' : 'Chưa/Đã hết hạn'}
                    color={isActive(delegation) ? 'success' : 'default'}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <IconButton
                    onClick={() => handleDownloadPdf(delegation.delegationId)}
                    title="Tải PDF"
                  >
                    <DownloadIcon />
                  </IconButton>
                  <IconButton
                    color="error"
                    onClick={() => handleCancel(delegation.delegationId)}
                    title="Hủy ủy quyền"
                  >
                    <CancelIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Tạo ủy quyền mới</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              select
              label="Người được ủy quyền"
              required
              value={formData.delegateeId}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  delegateeId: parseInt(e.target.value),
                })
              }
              fullWidth
            >
              {users.map((user) => (
                <MenuItem key={user.id} value={user.id}>
                  {user.fullName || user.username} ({user.email})
                </MenuItem>
              ))}
            </TextField>
            <TextField
              label="Từ ngày"
              type="date"
              required
              value={formData.startDate}
              onChange={(e) =>
                setFormData({ ...formData, startDate: e.target.value })
              }
              InputLabelProps={{ shrink: true }}
            />
            <TextField
              label="Đến ngày"
              type="date"
              required
              value={formData.endDate}
              onChange={(e) =>
                setFormData({ ...formData, endDate: e.target.value })
              }
              InputLabelProps={{ shrink: true }}
            />
            <TextField
              label="Lý do"
              multiline
              rows={3}
              value={formData.reason}
              onChange={(e) =>
                setFormData({ ...formData, reason: e.target.value })
              }
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Hủy</Button>
          <Button
            onClick={handleCreate}
            variant="contained"
            disabled={
              !formData.delegateeId ||
              !formData.startDate ||
              !formData.endDate
            }
          >
            Tạo
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}

