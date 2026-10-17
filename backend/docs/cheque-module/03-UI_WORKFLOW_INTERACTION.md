# UI Workflow & Interaction Document
# وثيقة سير العمل وتفاعل واجهة المستخدم

**Version:** 2.0
**Date:** 2025-12-06
**Module:** Treasury - Cheque Management Frontend

---

## 1. Overview

This document defines the complete UI workflow, button behaviors, form fields, and interaction patterns for the Cheque Management module in the frontend application.

---

## 2. Main Page Layout

### 2.1 Page Structure

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CHEQUES PAGE                                 │
├─────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │                    FILTER & ACTION BAR                          │ │
│ │  [Search...] [Status ▼] [Bank ▼] [Date Range] [+ New Cheque]   │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│                                                                     │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │                    STATISTICS CARDS                             │ │
│ │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │ │
│ │  │ Issued   │ │Presented │ │ Cleared  │ │ Bounced  │           │ │
│ │  │   15     │ │    8     │ │   120    │ │    3     │           │ │
│ │  │ 150,000  │ │  80,000  │ │1,200,000 │ │  30,000  │           │ │
│ │  └──────────┘ └──────────┘ └──────────┘ └──────────┘           │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│                                                                     │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │                    CHEQUES TABLE                                │ │
│ │ ┌─────┬────────┬─────────┬─────────┬────────┬────────┬───────┐ │ │
│ │ │  □  │ Number │ Payee   │ Amount  │  Due   │ Status │Actions│ │ │
│ │ ├─────┼────────┼─────────┼─────────┼────────┼────────┼───────┤ │ │
│ │ │  □  │CHQ-001 │ ABC Co  │ 10,000  │ Feb 15 │Issued  │[···]  │ │ │
│ │ │  □  │CHQ-002 │ XYZ Ltd │ 25,000  │ Feb 20 │Present │[···]  │ │ │
│ │ └─────┴────────┴─────────┴─────────┴────────┴────────┴───────┘ │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│                                                                     │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │                    PAGINATION                                   │ │
│ │              [< Prev] [1] [2] [3] [Next >]                      │ │
│ └─────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Status Indicators

| Status | Color | Badge Style | Icon |
|--------|-------|-------------|------|
| Issued | Blue | `bg-blue-100 text-blue-800` | 📝 |
| Presented | Orange | `bg-orange-100 text-orange-800` | 🏦 |
| Cleared | Green | `bg-green-100 text-green-800` | ✅ |
| Bounced | Red | `bg-red-100 text-red-800` | ⚠️ |
| Cancelled | Gray | `bg-gray-100 text-gray-800` | 🚫 |

---

## 3. Action Buttons Logic

### 3.1 Button Definitions

| Button | Label (EN) | Label (AR) | Action |
|--------|------------|------------|--------|
| Present | Present | تقديم | Submit cheque to bank |
| Clear | Clear | تصفية | Mark as cleared |
| Bounce | Bounce | ارتجاع | Mark as bounced |
| Cancel | Cancel | إلغاء | Cancel cheque |
| Edit | Edit | تعديل | Edit cheque details |
| Delete | Delete | حذف | Delete cheque |
| View | View | عرض | View cheque details |
| Print | Print | طباعة | Print cheque |

### 3.2 Visibility Conditions

```typescript
// Button visibility based on status
const actionConditions = {
  canEdit: (cheque) => cheque.status === ChequeStatus.Issued,
  canDelete: (cheque) => cheque.status === ChequeStatus.Issued,
  canPresent: (cheque) =>
    cheque.status === ChequeStatus.Issued ||
    cheque.status === ChequeStatus.Bounced,
  canClear: (cheque) => cheque.status === ChequeStatus.Presented,
  canBounce: (cheque) => cheque.status === ChequeStatus.Presented,
  canCancel: (cheque) =>
    cheque.status === ChequeStatus.Issued ||
    cheque.status === ChequeStatus.Presented ||
    cheque.status === ChequeStatus.Bounced,
  canView: () => true,
  canPrint: () => true
};
```

### 3.3 Button States Matrix

| Status | Edit | Delete | Present | Clear | Bounce | Cancel | View | Print |
|--------|------|--------|---------|-------|--------|--------|------|-------|
| Issued | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ✅ | ✅ |
| Presented | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Cleared | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |
| Bounced | ❌ | ❌ | ✅ | ❌ | ❌ | ✅ | ✅ | ✅ |
| Cancelled | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |

---

## 4. Required Confirmations

### 4.1 Present Confirmation

```typescript
// Present confirmation dialog
{
  title: "Present Cheque | تقديم الشيك",
  message: "Are you sure you want to present this cheque to the bank?",
  messageAr: "هل أنت متأكد من تقديم هذا الشيك للبنك؟",
  confirmText: "Present | تقديم",
  cancelText: "Cancel | إلغاء",
  type: "info"
}
```

### 4.2 Clear Confirmation

```typescript
// Clear confirmation dialog
{
  title: "Clear Cheque | تصفية الشيك",
  message: "Confirm that the bank has cleared this cheque?",
  messageAr: "تأكيد أن البنك قد صفى هذا الشيك؟",
  confirmText: "Clear | تصفية",
  cancelText: "Cancel | إلغاء",
  type: "success"
}
```

### 4.3 Bounce Confirmation

```typescript
// Bounce confirmation dialog - requires additional input
{
  title: "Bounce Cheque | ارتجاع الشيك",
  fields: [
    {
      name: "reason",
      label: "Bounce Reason | سبب الارتجاع",
      type: "select",
      required: true,
      options: [
        { value: "NSF", labelEn: "Insufficient Funds", labelAr: "رصيد غير كافي" },
        { value: "CLOSED", labelEn: "Account Closed", labelAr: "حساب مغلق" },
        { value: "SIGNATURE", labelEn: "Signature Mismatch", labelAr: "توقيع غير مطابق" },
        { value: "DATE", labelEn: "Date Error", labelAr: "خطأ في التاريخ" },
        { value: "STOPPED", labelEn: "Payment Stopped", labelAr: "إيقاف الدفع" },
        { value: "OTHER", labelEn: "Other", labelAr: "أخرى" }
      ]
    },
    {
      name: "bankFees",
      label: "Bank Fees | رسوم البنك",
      type: "number",
      required: false,
      default: 0
    },
    {
      name: "notes",
      label: "Notes | ملاحظات",
      type: "textarea",
      required: false
    }
  ],
  confirmText: "Bounce | ارتجاع",
  cancelText: "Cancel | إلغاء",
  type: "warning"
}
```

### 4.4 Cancel Confirmation

```typescript
// Cancel confirmation dialog - requires reason
{
  title: "Cancel Cheque | إلغاء الشيك",
  fields: [
    {
      name: "reason",
      label: "Cancellation Reason | سبب الإلغاء",
      type: "textarea",
      required: true,
      minLength: 10
    }
  ],
  confirmText: "Cancel Cheque | إلغاء الشيك",
  cancelText: "Go Back | العودة",
  type: "danger"
}
```

---

## 5. Form Fields per Transition

### 5.1 Create/Edit Form Fields

```typescript
interface ChequeFormFields {
  // Required Fields
  chequeNumber: {
    type: 'text',
    required: true,
    label: { en: 'Cheque Number', ar: 'رقم الشيك' },
    maxLength: 50
  },
  chequeType: {
    type: 'select',
    required: true,
    label: { en: 'Cheque Type', ar: 'نوع الشيك' },
    options: [
      { value: 0, labelEn: 'Outgoing', labelAr: 'صادر' },
      { value: 1, labelEn: 'Incoming', labelAr: 'وارد' }
    ]
  },
  bankAccountId: {
    type: 'select',
    required: true,
    label: { en: 'Bank Account', ar: 'الحساب البنكي' },
    dataSource: 'bankAccounts'
  },
  payeeName: {
    type: 'text',
    required: true,
    label: { en: 'Payee Name', ar: 'اسم المستفيد' },
    maxLength: 200
  },
  amount: {
    type: 'number',
    required: true,
    label: { en: 'Amount', ar: 'المبلغ' },
    min: 0.01,
    precision: 2
  },
  currency: {
    type: 'select',
    required: true,
    label: { en: 'Currency', ar: 'العملة' },
    default: 'SAR'
  },
  issueDate: {
    type: 'date',
    required: true,
    label: { en: 'Issue Date', ar: 'تاريخ الإصدار' }
  },
  dueDate: {
    type: 'date',
    required: true,
    label: { en: 'Due Date', ar: 'تاريخ الاستحقاق' },
    validation: 'mustBeAfterOrEqual:issueDate'
  },

  // Optional Fields (Incoming Cheques)
  drawerName: {
    type: 'text',
    required: false,
    showWhen: 'chequeType === 1',
    label: { en: 'Drawer Name', ar: 'اسم الساحب' }
  },
  drawerBankName: {
    type: 'text',
    required: false,
    showWhen: 'chequeType === 1',
    label: { en: 'Drawer Bank', ar: 'بنك الساحب' }
  },

  // Optional Fields (All Cheques)
  referenceNo: {
    type: 'text',
    required: false,
    label: { en: 'Reference No', ar: 'رقم المرجع' }
  },
  memo: {
    type: 'textarea',
    required: false,
    label: { en: 'Memo', ar: 'الملاحظة' }
  },
  costCenterId: {
    type: 'select',
    required: false,
    label: { en: 'Cost Center', ar: 'مركز التكلفة' }
  },
  departmentId: {
    type: 'select',
    required: false,
    label: { en: 'Department', ar: 'القسم' }
  },
  projectId: {
    type: 'select',
    required: false,
    label: { en: 'Project', ar: 'المشروع' }
  }
}
```

### 5.2 Present Form Fields

```typescript
// Minimal form - just confirmation
interface PresentFormFields {
  presentedDate: {
    type: 'date',
    required: true,
    label: { en: 'Presentation Date', ar: 'تاريخ التقديم' },
    default: 'today'
  },
  notes: {
    type: 'textarea',
    required: false,
    label: { en: 'Notes', ar: 'ملاحظات' }
  }
}
```

### 5.3 Clear Form Fields

```typescript
interface ClearFormFields {
  clearedDate: {
    type: 'date',
    required: true,
    label: { en: 'Cleared Date', ar: 'تاريخ التصفية' },
    default: 'today'
  },
  bankReference: {
    type: 'text',
    required: false,
    label: { en: 'Bank Reference', ar: 'مرجع البنك' }
  }
}
```

### 5.4 Bounce Form Fields

```typescript
interface BounceFormFields {
  bounceReason: {
    type: 'select',
    required: true,
    label: { en: 'Bounce Reason', ar: 'سبب الارتجاع' }
  },
  bouncedDate: {
    type: 'date',
    required: true,
    label: { en: 'Bounced Date', ar: 'تاريخ الارتجاع' },
    default: 'today'
  },
  bankFees: {
    type: 'number',
    required: false,
    label: { en: 'Bank Fees', ar: 'رسوم البنك' },
    default: 0,
    min: 0
  },
  notes: {
    type: 'textarea',
    required: false,
    label: { en: 'Notes', ar: 'ملاحظات' }
  }
}
```

---

## 6. Timeline / History Panel

### 6.1 Timeline Design

```
┌─────────────────────────────────────────────────────────────────────┐
│                    CHEQUE HISTORY TIMELINE                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ● Cleared                                        Feb 15, 2025      │
│  │ Cheque cleared by bank                         10:30 AM          │
│  │ User: ahmed.ali                                                  │
│  │                                                                  │
│  ● Presented                                      Feb 10, 2025      │
│  │ Cheque submitted to bank for clearing          09:15 AM          │
│  │ User: mohammed.hassan                                            │
│  │                                                                  │
│  ● Issued                                         Feb 01, 2025      │
│    Cheque issued to ABC Company                   02:45 PM          │
│    User: fatima.omar                                                │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 6.2 Timeline Component Structure

```typescript
interface TimelineItem {
  id: string;
  status: ChequeStatus;
  statusLabel: { en: string; ar: string };
  description: { en: string; ar: string };
  timestamp: Date;
  userId: string;
  userName: string;
  reason?: string;
  notes?: string;
  metadata?: {
    bankFees?: number;
    bankReference?: string;
  };
}

// Timeline component props
interface ChequeTimelineProps {
  chequeId: string;
  items: TimelineItem[];
  currentStatus: ChequeStatus;
  locale: 'en' | 'ar';
}
```

### 6.3 Timeline Item Styling

```typescript
const timelineItemStyles = {
  [ChequeStatus.Issued]: {
    color: 'blue',
    icon: 'DocumentTextIcon',
    connector: 'border-blue-300'
  },
  [ChequeStatus.Presented]: {
    color: 'orange',
    icon: 'BuildingLibraryIcon',
    connector: 'border-orange-300'
  },
  [ChequeStatus.Cleared]: {
    color: 'green',
    icon: 'CheckCircleIcon',
    connector: 'border-green-300'
  },
  [ChequeStatus.Bounced]: {
    color: 'red',
    icon: 'ExclamationTriangleIcon',
    connector: 'border-red-300'
  },
  [ChequeStatus.Cancelled]: {
    color: 'gray',
    icon: 'XCircleIcon',
    connector: 'border-gray-300'
  }
};
```

---

## 7. API Calling Patterns

### 7.1 React Query Hooks

```typescript
// Queries
export const useCheques = (params: ChequeQueryParams) => {
  return useQuery({
    queryKey: ['cheques', params],
    queryFn: () => listCheques(params),
    staleTime: 30000 // 30 seconds
  });
};

export const useCheque = (id: string) => {
  return useQuery({
    queryKey: ['cheque', id],
    queryFn: () => getCheque(id),
    enabled: !!id
  });
};

export const useChequeHistory = (chequeId: string) => {
  return useQuery({
    queryKey: ['cheque-history', chequeId],
    queryFn: () => getChequeHistory(chequeId),
    enabled: !!chequeId
  });
};

// Mutations
export const useCreateCheque = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createCheque,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cheques'] });
      toast.success('Cheque created successfully');
    },
    onError: (error) => {
      toast.error(error.message);
    }
  });
};

export const usePresentCheque = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id }: { id: string }) => presentCheque(id),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['cheques'] });
      queryClient.invalidateQueries({ queryKey: ['cheque', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['cheque-history', variables.id] });
      toast.success('Cheque presented successfully');
    }
  });
};

export const useClearCheque = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id }: { id: string }) => clearCheque(id),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['cheques'] });
      queryClient.invalidateQueries({ queryKey: ['cheque', variables.id] });
      toast.success('Cheque cleared successfully');
    }
  });
};

export const useBounceCheque = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason, bankFees }: BounceParams) =>
      bounceCheque(id, reason, bankFees),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['cheques'] });
      queryClient.invalidateQueries({ queryKey: ['cheque', variables.id] });
      toast.warning('Cheque marked as bounced');
    }
  });
};

export const useCancelCheque = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      cancelCheque(id, reason),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['cheques'] });
      queryClient.invalidateQueries({ queryKey: ['cheque', variables.id] });
      toast.info('Cheque cancelled');
    }
  });
};
```

### 7.2 API Service Functions

```typescript
// treasury.service.ts

const BASE_URL = '/erp/finance/treasury';

// Get cheques list
export const listCheques = (params?: ChequeQueryParams) => {
  return get(`${BASE_URL}/Cheque/list`, params);
};

// Get single cheque
export const getCheque = (id: string) => {
  return get(`${BASE_URL}/Cheque/${id}`);
};

// Get cheque history
export const getChequeHistory = (chequeId: string) => {
  return get(`${BASE_URL}/Cheque/${chequeId}/history`);
};

// Create cheque
export const createCheque = (data: CreateChequeForm) => {
  return post(`${BASE_URL}/Cheque`, data);
};

// Update cheque
export const updateCheque = (data: UpdateChequeForm) => {
  return put(`${BASE_URL}/Cheque`, data);
};

// Present cheque (Issued → Presented)
export const presentCheque = (chequeId: string) => {
  return put(`${BASE_URL}/Cheque/${chequeId}/present`);
};

// Clear cheque (Presented → Cleared)
export const clearCheque = (chequeId: string) => {
  return put(`${BASE_URL}/Cheque/${chequeId}/clear`);
};

// Bounce cheque (Presented → Bounced)
export const bounceCheque = (chequeId: string, reason: string, bankFees?: number) => {
  return put(`${BASE_URL}/Cheque/${chequeId}/bounce`, { reason, bankFees });
};

// Cancel cheque
export const cancelCheque = (chequeId: string, reason: string) => {
  return put(`${BASE_URL}/Cheque/${chequeId}/cancel`, { reason });
};

// Delete cheque
export const deleteCheque = (id: string) => {
  return del(`${BASE_URL}/Cheque/${id}`);
};
```

---

## 8. Error Handling

### 8.1 Error Types

```typescript
enum ChequeErrorCode {
  INVALID_STATUS_TRANSITION = 'INVALID_STATUS_TRANSITION',
  INSUFFICIENT_BALANCE = 'INSUFFICIENT_BALANCE',
  CHEQUE_NOT_FOUND = 'CHEQUE_NOT_FOUND',
  DUPLICATE_CHEQUE_NUMBER = 'DUPLICATE_CHEQUE_NUMBER',
  PERIOD_CLOSED = 'PERIOD_CLOSED',
  BANK_ACCOUNT_INACTIVE = 'BANK_ACCOUNT_INACTIVE',
  VALIDATION_ERROR = 'VALIDATION_ERROR'
}
```

### 8.2 Error Display

```typescript
const errorMessages: Record<ChequeErrorCode, { en: string; ar: string }> = {
  INVALID_STATUS_TRANSITION: {
    en: 'This action is not allowed for the current cheque status',
    ar: 'هذا الإجراء غير مسموح للحالة الحالية للشيك'
  },
  INSUFFICIENT_BALANCE: {
    en: 'Insufficient available balance in the bank account',
    ar: 'رصيد غير كافي في الحساب البنكي'
  },
  CHEQUE_NOT_FOUND: {
    en: 'Cheque not found',
    ar: 'الشيك غير موجود'
  },
  DUPLICATE_CHEQUE_NUMBER: {
    en: 'Cheque number already exists for this bank account',
    ar: 'رقم الشيك موجود بالفعل لهذا الحساب البنكي'
  },
  PERIOD_CLOSED: {
    en: 'The fiscal period is closed for this date',
    ar: 'الفترة المالية مغلقة لهذا التاريخ'
  },
  BANK_ACCOUNT_INACTIVE: {
    en: 'The bank account is not active',
    ar: 'الحساب البنكي غير نشط'
  },
  VALIDATION_ERROR: {
    en: 'Please check the form and correct any errors',
    ar: 'يرجى مراجعة النموذج وتصحيح الأخطاء'
  }
};
```

### 8.3 Toast Notifications

```typescript
// Success toasts
toast.success({
  create: { en: 'Cheque created successfully', ar: 'تم إنشاء الشيك بنجاح' },
  present: { en: 'Cheque presented to bank', ar: 'تم تقديم الشيك للبنك' },
  clear: { en: 'Cheque cleared successfully', ar: 'تم تصفية الشيك بنجاح' },
  cancel: { en: 'Cheque cancelled', ar: 'تم إلغاء الشيك' }
});

// Warning toasts
toast.warning({
  bounce: { en: 'Cheque has been bounced', ar: 'تم ارتجاع الشيك' },
  postDated: { en: 'This is a post-dated cheque', ar: 'هذا شيك مؤجل' }
});
```

---

## 9. Component Layout

### 9.1 File Structure

```
src/app/[locale]/(hydrogen)/finance/treasury/cheques/
├── page.tsx                    # Main cheques page
├── components/
│   ├── ChequeTable.tsx         # Data table component
│   ├── ChequeForm.tsx          # Create/Edit form
│   ├── ChequeDetails.tsx       # View cheque details
│   ├── ChequeTimeline.tsx      # History timeline
│   ├── ChequeActions.tsx       # Action buttons
│   ├── ChequeStatusBadge.tsx   # Status indicator
│   ├── ChequeFilters.tsx       # Filter controls
│   ├── ChequeStats.tsx         # Statistics cards
│   └── modals/
│       ├── PresentModal.tsx    # Present confirmation
│       ├── ClearModal.tsx      # Clear confirmation
│       ├── BounceModal.tsx     # Bounce with reason
│       └── CancelModal.tsx     # Cancel with reason
├── hooks/
│   ├── useCheques.ts           # React Query hooks
│   └── useChequeActions.ts     # Action handlers
├── types/
│   └── cheque.types.ts         # TypeScript interfaces
└── utils/
    └── cheque.helpers.ts       # Helper functions
```

### 9.2 Main Page Component

```typescript
// page.tsx
export default function ChequesPage() {
  const { t } = useTranslation();
  const [filters, setFilters] = useState<ChequeFilters>({});
  const [selectedCheque, setSelectedCheque] = useState<string | null>(null);
  const [modalType, setModalType] = useState<ModalType | null>(null);

  const { data: cheques, isLoading } = useCheques(filters);

  return (
    <PageContainer title={t('cheques.title')}>
      {/* Stats Cards */}
      <ChequeStats />

      {/* Filters & Actions */}
      <ChequeFilters
        filters={filters}
        onChange={setFilters}
        onCreateNew={() => setModalType('create')}
      />

      {/* Data Table */}
      <ChequeTable
        data={cheques?.data || []}
        loading={isLoading}
        onAction={(action, chequeId) => {
          setSelectedCheque(chequeId);
          setModalType(action);
        }}
      />

      {/* Modals */}
      <ChequeFormModal
        open={modalType === 'create' || modalType === 'edit'}
        chequeId={selectedCheque}
        onClose={() => setModalType(null)}
      />
      <PresentModal
        open={modalType === 'present'}
        chequeId={selectedCheque}
        onClose={() => setModalType(null)}
      />
      <ClearModal
        open={modalType === 'clear'}
        chequeId={selectedCheque}
        onClose={() => setModalType(null)}
      />
      <BounceModal
        open={modalType === 'bounce'}
        chequeId={selectedCheque}
        onClose={() => setModalType(null)}
      />
      <CancelModal
        open={modalType === 'cancel'}
        chequeId={selectedCheque}
        onClose={() => setModalType(null)}
      />
      <ChequeDetailsDrawer
        open={modalType === 'view'}
        chequeId={selectedCheque}
        onClose={() => setModalType(null)}
      />
    </PageContainer>
  );
}
```

---

## 10. Optimistic Updates

### 10.1 Status Change Optimistic Update

```typescript
// useChequeActions.ts
export const usePresentCheque = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: presentCheque,
    onMutate: async (chequeId) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['cheques'] });

      // Snapshot the previous value
      const previousCheques = queryClient.getQueryData(['cheques']);

      // Optimistically update
      queryClient.setQueryData(['cheques'], (old: any) => ({
        ...old,
        data: old.data.map((c: Cheque) =>
          c.id === chequeId
            ? { ...c, status: ChequeStatus.Presented, presentedDate: new Date() }
            : c
        )
      }));

      return { previousCheques };
    },
    onError: (err, chequeId, context) => {
      // Roll back on error
      queryClient.setQueryData(['cheques'], context?.previousCheques);
      toast.error('Failed to present cheque');
    },
    onSettled: () => {
      // Refetch after mutation
      queryClient.invalidateQueries({ queryKey: ['cheques'] });
    }
  });
};
```

---

## 11. Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-01 | System | Initial document |
| 2.0 | 2025-12-06 | Claude | Complete UI workflow specification |
