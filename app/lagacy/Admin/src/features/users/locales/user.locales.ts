import type { FeatureLocales } from '@/shared/locales/locale.types';

export interface UserLocales extends FeatureLocales {
  confirm: {
    delete_header: string;
    delete_message: string;
    accept_label: string;
    reject_label: string;
    reset_password_header: string;
    reset_password_message: string;
  };
  tabs: {
    details: string;
    roles: string;
    permissions: string;
    security: string;
  };
  security: {
    status_title: string;
    actions_title: string;
    reset_password: string;
    unlock_account: string;
    manual_verify: string;
    lockout_end: string;
    failed_attempts: string;
    email_verified: string;
    phone_verified: string;
  };
}

export const userLocales: UserLocales = {
  titles: {
    list: 'Staff Management',
    customers: 'Customer Management',
    create: 'Invite Staff',
    edit: 'Edit User',
    basic_info: 'User Details',
    permissions: 'Direct Permissions',
    roles: 'Assigned Roles',
    security: 'Security & access',
  },
  descriptions: {
    list: 'Manage administrative users and permissions.',
    customers: 'View and manage registered customers and their account status.',
    create: 'Invite a new staff member to the admin panel.',
  },
  tabs: {
    details: 'Profile',
    roles: 'Roles',
    permissions: 'Permissions',
    security: 'Security',
  },
  table: {
    user: 'User',
    roles: 'Roles',
    status: 'Status',
    joined: 'Joined',
    actions: 'Actions',
    clear_filter: 'Clear Filters'
  },
  labels: {
    full_name: 'Full Name',
    email: 'Email',
    roles: 'Roles',
    status: 'Status',
    username: 'Username',
    first_name: 'First Name',
    last_name: 'Last Name',
  },
  actions: {
    new: 'Invite Staff',
    save: 'Save User',
    cancel: 'Cancel',
    delete: 'Remove Access',
    edit: 'Edit Profile',
    reset_password: 'Reset Password',
    unlock: 'Unlock Account',
    verify: 'Verify Account',
  },
  security: {
    status_title: 'Account Security Status',
    actions_title: 'Administrative Actions',
    reset_password: 'Force Password Reset',
    unlock_account: 'Unlock Account',
    manual_verify: 'Manual Verification',
    lockout_end: 'Lockout Ends',
    failed_attempts: 'Failed Attempts',
    email_verified: 'Email Verified',
    phone_verified: 'Phone Verified',
  },
  placeholders: {
    search: 'Search by name or email...',
    name: 'e.g. John Doe',
    email: 'e.g. john@resys.shop',
  },
  messages: {
    create_success: 'User invited successfully.',
    update_success: 'User updated successfully.',
    delete_success: 'User access removed successfully.',
    reset_password_success: 'Password has been reset successfully.',
    unlock_success: 'Account has been unlocked.',
    verify_success: 'Account verification updated.',
    empty_list: 'No users found matching your criteria.',
    loading: 'Loading users...',
  },
  confirm: {
    delete_header: 'Confirm Removal',
    delete_message: 'Are you sure you want to remove staff access for "{email}"? This action cannot be undone.',
    accept_label: 'Remove Access',
    reject_label: 'Cancel',
    reset_password_header: 'Reset Password',
    reset_password_message: 'Are you sure you want to force a password reset for this user? A temporary password will be required.',
  },
  common: {
    success: 'Success',
    error: 'Error',
    warning: 'Warning',
  }
};