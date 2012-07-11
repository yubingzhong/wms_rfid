﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.RfidWms.DBModel.Ef.Models.Wms;
using THOK.Authority.Dal.Interfaces.Wms;
using THOK.Authority.Dal.Infrastructure.RepositoryContext;
using THOK.Authority.Dal.Interfaces.Authority;

namespace THOK.Authority.Dal.EntityRepository.Wms
{
    public class UnitRepository:RepositoryBase<Unit>,IUnitRepository
    {
        public UnitRepository()
            : this(new AuthorityRepositoryContext())
        {
        }

        public UnitRepository(IAuthorityRepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
    }
}
