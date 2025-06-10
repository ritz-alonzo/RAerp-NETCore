using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.EntityTypes.Helpers
{
    public static class EntityTypeMessages
    {
        #region Error messages

        public const string EntityTypeIdNotExists = "Entity Type Id cannot be null";

        public const string EntityTypeNotExists = "Entity Type doesn't exists";

        public const string ParentEntityTypeIdNotExists = "Parent entity type Id doesn't exists";

        public const string ParentTypeNotExists = "Parent entity type doesn't exists";

        public const string EntityNameExists = "Entity Name already exists";

        public const string EntityNameRequired = "Entity Name is required";


        #region Base Entity Model

        public const string CodeRequired = "Code is required";

        public const string NameRequired = "Name is required";

        #endregion

        #endregion
    }
}
