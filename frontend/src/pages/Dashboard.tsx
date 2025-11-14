import { useEffect, useState } from 'react'
import {
  Container,
  Typography,
  Grid,
  Card,
  CardContent,
  Box,
  Alert,
} from '@mui/material'
import { useAuth } from '../contexts/AuthContext'
import api from '../services/api'
import { DelegationInfo } from '../types'

export default function Dashboard() {
  const { user } = useAuth()
  const [delegations, setDelegations] = useState<DelegationInfo[]>([])

  useEffect(() => {
    const fetchDelegations = async () => {
      try {
        const response = await api.get('/delegations/my-delegations')
        setDelegations(response.data)
      } catch (error) {
        console.error('Error fetching delegations:', error)
      }
    }

    fetchDelegations()
  }, [])

  return (
    <Container>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>

      <Grid container spacing={3} sx={{ mt: 2 }}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Thông tin người dùng
              </Typography>
              <Typography variant="body1">
                <strong>Tên đăng nhập:</strong> {user?.username}
              </Typography>
              <Typography variant="body1">
                <strong>Họ tên:</strong> {user?.fullName || 'N/A'}
              </Typography>
              <Typography variant="body1">
                <strong>Email:</strong> {user?.email || 'N/A'}
              </Typography>
              <Typography variant="body1">
                <strong>Loại tài khoản:</strong>{' '}
                {user?.isLocalUser ? 'Local' : 'Active Directory'}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Phòng ban
              </Typography>
              {user?.departments && user.departments.length > 0 ? (
                <Box>
                  {user.departments.map((dept) => (
                    <Typography key={dept.id} variant="body1">
                      {dept.name} {dept.role && `- ${dept.role.name}`}
                    </Typography>
                  ))}
                </Box>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  Chưa được phân vào phòng ban nào
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        {delegations.length > 0 && (
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Ủy quyền đang hoạt động
                </Typography>
                {delegations.map((delegation) => (
                  <Alert severity="info" key={delegation.delegationId} sx={{ mb: 1 }}>
                    Bạn đang được ủy quyền bởi{' '}
                    <strong>{delegation.delegator.fullName || delegation.delegator.username}</strong>{' '}
                    từ {new Date(delegation.startDate).toLocaleDateString('vi-VN')} đến{' '}
                    {new Date(delegation.endDate).toLocaleDateString('vi-VN')}
                  </Alert>
                ))}
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>
    </Container>
  )
}

