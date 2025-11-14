export interface UserInfo {
  id: number
  username: string
  email?: string
  fullName?: string
  isLocalUser: boolean
  departments: DepartmentInfo[]
}

export interface DepartmentInfo {
  id: number
  name: string
  code?: string
  role?: RoleInfo
}

export interface RoleInfo {
  id: number
  name: string
}

export interface Department {
  id: number
  name: string
  code?: string
  description?: string
  isActive: boolean
  createdAt: string
}

export interface Role {
  id: number
  name: string
  description?: string
  departmentId: number
  isActive: boolean
}

export interface DelegationInfo {
  delegationId: number
  delegator: UserInfo
  startDate: string
  endDate: string
  reason?: string
}

export interface CreateDelegationRequest {
  delegateeId: number
  startDate: string
  endDate: string
  reason?: string
}

export interface CreateLocalUserRequest {
  username: string
  password: string
  email?: string
  fullName?: string
}

