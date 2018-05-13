using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PrimeApps.Model.Common.Resources
{
    public enum EmailResource
    {
        [ResourceName("email-reset")]
        EmailReset,
        [ResourceName("email-status-successful")]
        EMailStatusSuccessful,

        [ResourceName("email-status-failed")]
        EMailStatusFailed,

        [ResourceName("event-reminder")]
        EventReminder,

        [ResourceName("task-reminder")]
        TaskReminder,

        [ResourceName("call-reminder")]
        CallReminder,

        [ResourceName("task-owner-changed-notification")]
        TaskOwnerChangedNotification,

        [ResourceName("owner-changed-notification")]
        OwnerChangedNotification,

        [ResourceName("task-assigned-notification")]
        TaskAssignedNotification,

        [ResourceName("approval-process-create-notification")]
        ApprovalProcessCreateNotification,

        [ResourceName("approval-process-update-notification")]
        ApprovalProcessUpdateNotification,

        [ResourceName("approval-process-reject-notification")]
        ApprovalProcessRejectNotification,

        [ResourceName("approval-process-update-reject-notification")]
        ApprovalProcessUpdateRejectNotification,

        [ResourceName("approval-process-approve-notification")]
        ApprovalProcessApproveNotification,

        [ResourceName("workflow-notification")]
        WorkflowNotification,

        [ResourceName("trial-expire-mail")]
        TrialExpireMail
    }
}
