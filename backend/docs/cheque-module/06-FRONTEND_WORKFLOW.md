# Frontend Workflow Enhancement Design
# تصميم تحسين سير العمل للواجهة

**Version:** 1.0
**Date:** 2025-12-06
**Module:** Treasury - Cheque Management Frontend

---

## 1. Overview

This document details the frontend enhancements required to implement the complete cheque lifecycle with status history timeline.

---

## 2. Current Page Structure Analysis

### 2.1 Existing Features (Keep)
- ✅ List view with pagination
- ✅ Create/Edit/View modals
- ✅ Action buttons (Present, Clear, Bounce, Cancel)
- ✅ Status badges with colors
- ✅ Search and filter panel
- ✅ Overdue indicator
- ✅ Responsive design

### 2.2 Features to Add
- 🆕 Status History Timeline component
- 🆕 Statistics Cards
- 🆕 Bank Fees input in Bounce modal
- 🆕 Confirmation dialogs for Present/Clear
- 🆕 Drawer fields for Incoming cheques

### 2.3 Features to Fix
- 🔧 canPresent should allow Bounced status
- 🔧 canCancel should allow Bounced status

---

## 3. Page Layout Enhancement

### 3.1 Target Layout

```
┌─────────────────────────────────────────────────────────────────────┐
│                       PAGE HEADER                                    │
│  Cheques | الشيكات                            [Search] [Refresh] [+]│
├─────────────────────────────────────────────────────────────────────┤
│                    STATISTICS CARDS (NEW)                           │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│  │ Total   │ │ Issued  │ │Presented│ │ Cleared │ │ Bounced │       │
│  │   156   │ │   15    │ │    8    │ │   120   │ │    3    │       │
│  │1,560,000│ │150,000  │ │80,000   │ │1,200,000│ │30,000   │       │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘ └─────────┘       │
├─────────────────────────────────────────────────────────────────────┤
│                    FILTER PANEL (Existing)                          │
│  [Cheque #] [Type ▼] [Status ▼] [Bank ▼]     [Reset] [Search]      │
├─────────────────────────────────────────────────────────────────────┤
│                    DATA TABLE (Existing)                            │
│  ┌──────┬────────┬────────┬─────────┬────────┬────────┬──────────┐│
│  │Cheque│ Type   │ Payee  │ Amount  │Due Date│ Status │ Actions  ││
│  ├──────┼────────┼────────┼─────────┼────────┼────────┼──────────┤│
│  │...   │...     │...     │...      │...     │...     │[Actions] ││
│  └──────┴────────┴────────┴─────────┴────────┴────────┴──────────┘│
│                    PAGINATION                                       │
└─────────────────────────────────────────────────────────────────────┘
```

### 3.2 View Details with Timeline

```
┌─────────────────────────────────────────────────────────────────────┐
│                    CHEQUE DETAILS DRAWER                            │
│                                                              [Close]│
├─────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────────┐│
│ │ CHEQUE INFO                                                      ││
│ │ Number: CHQ-001          Type: Outgoing                          ││
│ │ Payee: ABC Company       Amount: SAR 10,000                      ││
│ │ Bank: Al Rajhi Bank      Status: [Cleared ✓]                     ││
│ │ Issue Date: Jan 15       Due Date: Feb 15                        ││
│ └─────────────────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────────┐│
│ │ STATUS TIMELINE (NEW)                                            ││
│ │                                                                  ││
│ │  ● Cleared                                     Feb 20, 2025      ││
│ │  │ Cheque cleared successfully                 10:30 AM          ││
│ │  │ By: ahmed.ali                                                 ││
│ │  │                                                               ││
│ │  ● Presented                                   Feb 15, 2025      ││
│ │  │ Cheque presented to bank                    09:15 AM          ││
│ │  │ By: mohammed.hassan                                           ││
│ │  │                                                               ││
│ │  ● Issued                                      Jan 15, 2025      ││
│ │    Cheque issued                               02:45 PM          ││
│ │    By: fatima.omar                                               ││
│ │                                                                  ││
│ └─────────────────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────────┐│
│ │ ACTIONS                                                          ││
│ │ [Print]                                                          ││
│ └─────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────┘
```

---

## 4. New Components Design

### 4.1 ChequeStats Component

```typescript
// components/ChequeStats.tsx

interface ChequeStatsProps {
  isArabic: boolean;
  loading?: boolean;
}

interface StatCardProps {
  title: string;
  titleAr: string;
  count: number;
  amount: number;
  currency: string;
  color: string;
  icon: React.ReactNode;
}

const statCards = [
  {
    key: 'total',
    titleEn: 'Total',
    titleAr: 'الإجمالي',
    color: 'bg-gray-100 dark:bg-gray-700',
    textColor: 'text-gray-800 dark:text-gray-200'
  },
  {
    key: 'issued',
    titleEn: 'Issued',
    titleAr: 'صادرة',
    color: 'bg-blue-100 dark:bg-blue-900/30',
    textColor: 'text-blue-800 dark:text-blue-300'
  },
  {
    key: 'presented',
    titleEn: 'Presented',
    titleAr: 'مقدمة',
    color: 'bg-yellow-100 dark:bg-yellow-900/30',
    textColor: 'text-yellow-800 dark:text-yellow-300'
  },
  {
    key: 'cleared',
    titleEn: 'Cleared',
    titleAr: 'مقاصة',
    color: 'bg-green-100 dark:bg-green-900/30',
    textColor: 'text-green-800 dark:text-green-300'
  },
  {
    key: 'bounced',
    titleEn: 'Bounced',
    titleAr: 'مرتجعة',
    color: 'bg-red-100 dark:bg-red-900/30',
    textColor: 'text-red-800 dark:text-red-300'
  }
];
```

### 4.2 ChequeTimeline Component

```typescript
// components/ChequeTimeline.tsx

interface TimelineItem {
  id: string;
  status: number;
  statusLabel: string;
  statusLabelAr: string;
  statusColor: string;
  statusIcon: string;
  description: string;
  descriptionAr: string;
  timestamp: string;
  userName: string;
  reason?: string;
  bankFees?: number;
  bankReference?: string;
  glJournalId?: string;
}

interface ChequeTimelineProps {
  chequeId: string;
  items: TimelineItem[];
  isArabic: boolean;
  loading?: boolean;
}

// Status color mapping
const statusColors = {
  issued: {
    bg: 'bg-blue-500',
    ring: 'ring-blue-100',
    connector: 'border-blue-200'
  },
  presented: {
    bg: 'bg-yellow-500',
    ring: 'ring-yellow-100',
    connector: 'border-yellow-200'
  },
  cleared: {
    bg: 'bg-green-500',
    ring: 'ring-green-100',
    connector: 'border-green-200'
  },
  bounced: {
    bg: 'bg-red-500',
    ring: 'ring-red-100',
    connector: 'border-red-200'
  },
  cancelled: {
    bg: 'bg-gray-500',
    ring: 'ring-gray-100',
    connector: 'border-gray-200'
  }
};
```

### 4.3 Enhanced Bounce Modal

```typescript
// Modal with Bank Fees Input

interface BounceModalProps {
  isOpen: boolean;
  chequeId: string;
  onClose: () => void;
  onConfirm: (reason: string, bankFees: number) => void;
  loading: boolean;
  isArabic: boolean;
}

// Form fields:
// - reason (textarea, required)
// - bankFees (number input, optional, default 0)
// - notes (textarea, optional)
```

### 4.4 Confirmation Modal

```typescript
// Generic confirmation modal for Present/Clear

interface ConfirmModalProps {
  isOpen: boolean;
  title: string;
  message: string;
  confirmText: string;
  cancelText: string;
  variant: 'info' | 'warning' | 'danger' | 'success';
  onConfirm: () => void;
  onCancel: () => void;
  loading?: boolean;
}
```

---

## 5. API Integration

### 5.1 New API Functions

```typescript
// Add to treasury.service.ts

/**
 * Get cheque status history
 */
export function getChequeHistory(chequeId: string) {
  return get(`${BASE_URL}/Cheque/${chequeId}/history`);
}

/**
 * Get cheque timeline for UI display
 */
export function getChequeTimeline(chequeId: string) {
  return get(`${BASE_URL}/Cheque/${chequeId}/timeline`);
}

/**
 * Get cheque statistics
 */
export function getChequeStatistics(params?: { bankAccountId?: string }) {
  return get(`${BASE_URL}/Cheque/statistics`, params);
}
```

### 5.2 Update Existing Functions

```typescript
// Update bounceCheque to include bankFees
export function bounceCheque(chequeId: string, reason: string, bankFees?: number) {
  return put(`${BASE_URL}/Cheque/${chequeId}/bounce`, { reason, bankFees });
}
```

---

## 6. Code Fixes Required

### 6.1 Fix Action Conditions

```typescript
// BEFORE (page.tsx line 439-444)
const canPresent = (cheque: Cheque) => cheque.status === 0; // Only Issued
const canCancel = (cheque: Cheque) => cheque.status === 0 || cheque.status === 1;

// AFTER
const canPresent = (cheque: Cheque) =>
  cheque.status === 0 || cheque.status === 3; // Issued or Bounced (re-present)

const canCancel = (cheque: Cheque) =>
  cheque.status === 0 || cheque.status === 1 || cheque.status === 3; // Issued, Presented, or Bounced
```

### 6.2 Add Incoming Cheque Fields

```typescript
// In Create/Edit form, add conditional fields for incoming cheques
{chequeType === 1 && (
  <>
    <div>
      <label className="block text-sm font-medium mb-1">
        {isArabic ? 'اسم الساحب' : 'Drawer Name'}
      </label>
      <input
        type="text"
        className="w-full rounded-md border px-3 py-2 text-sm dark:bg-gray-700 dark:border-gray-600"
        value={drawerName}
        onChange={(e) => setDrawerName(e.target.value)}
      />
    </div>
    <div>
      <label className="block text-sm font-medium mb-1">
        {isArabic ? 'بنك الساحب' : 'Drawer Bank'}
      </label>
      <input
        type="text"
        className="w-full rounded-md border px-3 py-2 text-sm dark:bg-gray-700 dark:border-gray-600"
        value={drawerBankName}
        onChange={(e) => setDrawerBankName(e.target.value)}
      />
    </div>
  </>
)}
```

### 6.3 Add Bank Fees to Bounce Modal

```typescript
// Update action modal for bounce
{actionType === 'bounce' && (
  <div className="mb-4">
    <label className="block text-sm font-medium mb-1">
      {isArabic ? 'رسوم البنك' : 'Bank Fees'}
    </label>
    <input
      type="number"
      min="0"
      step="0.01"
      className="w-full rounded-md border px-3 py-2 text-sm dark:bg-gray-700 dark:border-gray-600"
      value={bankFees}
      onChange={(e) => setBankFees(Number(e.target.value))}
      placeholder="0.00"
    />
  </div>
)}
```

---

## 7. State Management

### 7.1 New State Variables

```typescript
// Add to page.tsx
const [showTimeline, setShowTimeline] = useState(false);
const [timelineData, setTimelineData] = useState<TimelineItem[]>([]);
const [timelineLoading, setTimelineLoading] = useState(false);
const [bankFees, setBankFees] = useState(0);
const [drawerName, setDrawerName] = useState('');
const [drawerBankName, setDrawerBankName] = useState('');
const [statistics, setStatistics] = useState<ChequeStats | null>(null);
```

### 7.2 Load Timeline Data

```typescript
const loadTimeline = async (chequeId: string) => {
  setTimelineLoading(true);
  try {
    const res = await getChequeTimeline(chequeId);
    if (res.code === 200) {
      setTimelineData(res.data || []);
    }
  } catch (error) {
    console.error('Error loading timeline:', error);
  } finally {
    setTimelineLoading(false);
  }
};

// Call when opening view modal
const openViewForm = async (cheque: Cheque) => {
  try {
    const [chequeRes, timelineRes] = await Promise.all([
      getCheque(cheque.id),
      getChequeTimeline(cheque.id)
    ]);

    if (chequeRes.code === 200 && chequeRes.data) {
      setSelectedCheque(chequeRes.data);
      setTimelineData(timelineRes.data || []);
      setModalMode('view');
      setDialogTitle(isArabic ? 'تفاصيل الشيك' : 'Cheque Details');
      setShowForm(true);
    }
  } catch (error) {
    console.error('Error loading cheque:', error);
    toast.error(isArabic ? 'فشل تحميل بيانات الشيك' : 'Failed to load cheque');
  }
};
```

---

## 8. Timeline Component Implementation

### 8.1 Timeline Rendering

```typescript
// ChequeTimeline.tsx

export function ChequeTimeline({ items, isArabic, loading }: ChequeTimelineProps) {
  if (loading) {
    return <TimelineSkeleton />;
  }

  if (items.length === 0) {
    return (
      <div className="text-center text-gray-500 py-4">
        {isArabic ? 'لا يوجد سجل' : 'No history'}
      </div>
    );
  }

  return (
    <div className="flow-root">
      <ul className="-mb-8">
        {items.map((item, index) => (
          <li key={item.id}>
            <div className="relative pb-8">
              {/* Connector line */}
              {index !== items.length - 1 && (
                <span
                  className={`absolute top-4 left-4 -ml-px h-full w-0.5 ${
                    getStatusColorClass(item.status).connector
                  }`}
                  aria-hidden="true"
                />
              )}

              <div className="relative flex space-x-3 rtl:space-x-reverse">
                {/* Status dot */}
                <div>
                  <span
                    className={`h-8 w-8 rounded-full flex items-center justify-center ring-4 ${
                      getStatusColorClass(item.status).bg
                    } ${getStatusColorClass(item.status).ring}`}
                  >
                    <StatusIcon status={item.status} />
                  </span>
                </div>

                {/* Content */}
                <div className="flex min-w-0 flex-1 justify-between space-x-4 pt-1.5">
                  <div>
                    <p className="font-medium text-gray-900 dark:text-gray-100">
                      {isArabic ? item.statusLabelAr : item.statusLabel}
                    </p>
                    <p className="text-sm text-gray-500">
                      {isArabic ? item.descriptionAr : item.description}
                    </p>
                    {item.reason && (
                      <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
                        <span className="font-medium">
                          {isArabic ? 'السبب: ' : 'Reason: '}
                        </span>
                        {item.reason}
                      </p>
                    )}
                    {item.bankFees && item.bankFees > 0 && (
                      <p className="mt-1 text-sm text-red-600">
                        <span className="font-medium">
                          {isArabic ? 'رسوم البنك: ' : 'Bank Fees: '}
                        </span>
                        {formatCurrency(item.bankFees, 'SAR')}
                      </p>
                    )}
                  </div>
                  <div className="whitespace-nowrap text-right text-sm text-gray-500">
                    <time dateTime={item.timestamp}>
                      {formatDateTime(item.timestamp, isArabic)}
                    </time>
                    <p className="text-xs">{item.userName}</p>
                  </div>
                </div>
              </div>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
```

---

## 9. Statistics Cards Implementation

### 9.1 Stats Component

```typescript
// ChequeStats.tsx

export function ChequeStats({ isArabic }: { isArabic: boolean }) {
  const [stats, setStats] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadStats();
  }, []);

  const loadStats = async () => {
    try {
      const res = await getChequeStatistics();
      if (res.code === 200) {
        setStats(res.data);
      }
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <StatsSkeleton />;

  return (
    <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-4">
      <StatCard
        title={isArabic ? 'الإجمالي' : 'Total'}
        count={stats?.total?.count || 0}
        amount={stats?.total?.amount || 0}
        color="gray"
      />
      <StatCard
        title={isArabic ? 'صادرة' : 'Issued'}
        count={stats?.issued?.count || 0}
        amount={stats?.issued?.amount || 0}
        color="blue"
      />
      <StatCard
        title={isArabic ? 'مقدمة' : 'Presented'}
        count={stats?.presented?.count || 0}
        amount={stats?.presented?.amount || 0}
        color="yellow"
      />
      <StatCard
        title={isArabic ? 'مقاصة' : 'Cleared'}
        count={stats?.cleared?.count || 0}
        amount={stats?.cleared?.amount || 0}
        color="green"
      />
      <StatCard
        title={isArabic ? 'مرتجعة' : 'Bounced'}
        count={stats?.bounced?.count || 0}
        amount={stats?.bounced?.amount || 0}
        color="red"
      />
    </div>
  );
}

function StatCard({ title, count, amount, color }: StatCardProps) {
  const colors = {
    gray: 'bg-gray-100 dark:bg-gray-700',
    blue: 'bg-blue-100 dark:bg-blue-900/30',
    yellow: 'bg-yellow-100 dark:bg-yellow-900/30',
    green: 'bg-green-100 dark:bg-green-900/30',
    red: 'bg-red-100 dark:bg-red-900/30'
  };

  return (
    <div className={`rounded-lg p-4 ${colors[color]}`}>
      <p className="text-sm font-medium text-gray-600 dark:text-gray-400">
        {title}
      </p>
      <p className="text-2xl font-bold">{count}</p>
      <p className="text-sm text-gray-500">
        {formatCurrency(amount, 'SAR')}
      </p>
    </div>
  );
}
```

---

## 10. Modal Enhancements

### 10.1 Enhanced View Modal with Timeline

```typescript
// In the view mode section of the form modal
{modalMode === 'view' && selectedCheque && (
  <>
    {/* Existing cheque details grid */}
    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-6">
      {/* ... existing fields ... */}
    </div>

    {/* NEW: Status Timeline */}
    <div className="border-t pt-4 mt-4 dark:border-gray-700">
      <h4 className="text-sm font-semibold mb-4">
        {isArabic ? 'سجل تغييرات الحالة' : 'Status History'}
      </h4>
      <ChequeTimeline
        items={timelineData}
        isArabic={isArabic}
        loading={timelineLoading}
      />
    </div>

    <div className="flex justify-end mt-4">
      <Button variant="secondary" onClick={() => setShowForm(false)}>
        {isArabic ? 'إغلاق' : 'Close'}
      </Button>
    </div>
  </>
)}
```

### 10.2 Enhanced Bounce Modal

```typescript
// Replace existing action modal
{showActionModal && (
  <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
    <div className="w-full max-w-md rounded-lg bg-white p-6 dark:bg-gray-800">
      <h3 className="mb-4 text-lg font-semibold">
        {actionType === 'bounce'
          ? (isArabic ? 'تسجيل ارتجاع الشيك' : 'Record Cheque Bounce')
          : (isArabic ? 'إلغاء الشيك' : 'Cancel Cheque')}
      </h3>

      {/* Reason field (both bounce and cancel) */}
      <div className="mb-4">
        <label className="block text-sm font-medium mb-1">
          {isArabic ? 'السبب' : 'Reason'} <span className="text-red-500">*</span>
        </label>
        <textarea
          className="w-full rounded-md border px-3 py-2 text-sm dark:bg-gray-700 dark:border-gray-600"
          rows={3}
          value={actionReason}
          onChange={(e) => setActionReason(e.target.value)}
          placeholder={isArabic ? 'أدخل السبب...' : 'Enter reason...'}
        />
      </div>

      {/* Bank fees (bounce only) */}
      {actionType === 'bounce' && (
        <div className="mb-4">
          <label className="block text-sm font-medium mb-1">
            {isArabic ? 'رسوم البنك' : 'Bank Fees'}
          </label>
          <input
            type="number"
            min="0"
            step="0.01"
            className="w-full rounded-md border px-3 py-2 text-sm dark:bg-gray-700 dark:border-gray-600"
            value={bankFees}
            onChange={(e) => setBankFees(Number(e.target.value))}
            placeholder="0.00"
          />
          <p className="mt-1 text-xs text-gray-500">
            {isArabic
              ? 'أدخل الرسوم البنكية إن وجدت'
              : 'Enter bank fees if applicable'}
          </p>
        </div>
      )}

      <div className="flex justify-end gap-2">
        <Button
          variant="secondary"
          onClick={() => {
            setShowActionModal(false);
            setBankFees(0);
          }}
        >
          {isArabic ? 'إلغاء' : 'Cancel'}
        </Button>
        <Button
          variant="primary"
          onClick={handleActionSubmit}
          disabled={actionLoading || !actionReason.trim()}
          className={actionType === 'bounce' ? 'bg-red-600 hover:bg-red-700' : ''}
        >
          {actionLoading
            ? (isArabic ? 'جاري المعالجة...' : 'Processing...')
            : (isArabic ? 'تأكيد' : 'Confirm')}
        </Button>
      </div>
    </div>
  </div>
)}
```

---

## 11. Implementation Sequence

### Phase 1: Code Fixes
1. Fix canPresent to allow Bounced status
2. Fix canCancel to allow Bounced status
3. Add bankFees to bounce modal
4. Update bounceCheque API call

### Phase 2: Timeline
1. Add getChequeTimeline API function
2. Create ChequeTimeline component
3. Integrate timeline in view modal
4. Load timeline data with cheque details

### Phase 3: Statistics
1. Add getChequeStatistics API (requires backend)
2. Create ChequeStats component
3. Add to page layout

### Phase 4: Form Enhancements
1. Add drawer fields for incoming cheques
2. Add confirmation dialogs (optional)

---

## 12. File Changes Summary

```
src/app/shared/services/finance/treasury.service.ts
  - Add getChequeTimeline()
  - Add getChequeStatistics()
  - Update bounceCheque() signature

src/app/[locale]/(hydrogen)/finance/treasury/cheques/page.tsx
  - Fix canPresent/canCancel conditions
  - Add timeline state and loading
  - Add bankFees state
  - Add drawer fields
  - Update openViewForm to load timeline
  - Update handleActionSubmit for bounce
  - Update modal rendering

src/app/[locale]/(hydrogen)/finance/treasury/cheques/components/ChequeTimeline.tsx (NEW)
  - Timeline component

src/app/[locale]/(hydrogen)/finance/treasury/cheques/components/ChequeStats.tsx (NEW)
  - Statistics cards component
```

---

## 13. Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-06 | Claude | Initial frontend design |
