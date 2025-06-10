using RA.Core.Models.BaseModels;
using RAerp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Factories.CoreFactories
{
    public static class BaseNotificationModelFactory<TModel> where TModel : BaseModel
    {
        public static TModel PrepareNotificationModel(TModel model, NotificationStatus notificationStatus, string message)
        {
            if (model == null)
                throw new ArgumentNullException(typeof(TModel).Name);

            switch (notificationStatus)
            {
                case NotificationStatus.Info:
                    model.JSNotificationFunction = "InfoNotif";
                    break;

                case NotificationStatus.Success:
                    model.JSNotificationFunction = "SuccessNotif";
                    break;

                case NotificationStatus.Warning:
                    model.JSNotificationFunction = "WarningNotif";
                    break;

                case NotificationStatus.Error:
                    model.JSNotificationFunction = "ErrorNotif";
                    break;
            }

            // will add more details here
            // but for now just show notification
            model.NotificationMessage = message;

            return model;
        }
    }
}
