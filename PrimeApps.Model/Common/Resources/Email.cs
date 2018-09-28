using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PrimeApps.Model.Common.Resources
{
    public enum EmailResource
    {
        [ResourceName("email_reset")]
        EmailReset,
        [ResourceName("email_status_successful")]
        EMailStatusSuccessful,

        [ResourceName("email_status_failed")]
        EMailStatusFailed,

        [ResourceName("event_reminder")]
        EventReminder,

        [ResourceName("task_reminder")]
        TaskReminder,

        [ResourceName("call_reminder")]
        CallReminder,

        [ResourceName("task_owner_changed_notification")]
        TaskOwnerChangedNotification,

        [ResourceName("owner_changed_notification")]
        OwnerChangedNotification,

        [ResourceName("task_assigned_notification")]
        TaskAssignedNotification,

        [ResourceName("approval_process_create_notification")]
        ApprovalProcessCreateNotification,

        [ResourceName("approval_process_update_notification")]
        ApprovalProcessUpdateNotification,

        [ResourceName("approval_process_reject_notification")]
        ApprovalProcessRejectNotification,

        [ResourceName("approval_process_update_reject_notification")]
        ApprovalProcessUpdateRejectNotification,

        [ResourceName("approval_process_approve_notification")]
        ApprovalProcessApproveNotification,

        [ResourceName("workflow_notification")]
        WorkflowNotification,

        [ResourceName("trial_expire_mail")]
        TrialExpireMail,

        [ResourceName("sms_status_failed")]
        SmsStatusFailed,

        [ResourceName("sms_status_successful")]
        SmsStatusSuccessful,

    }
}
