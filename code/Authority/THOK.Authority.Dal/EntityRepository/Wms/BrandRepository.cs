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
    public class BrandRepository:RepositoryBase<Brand>,IBrandRepository
    {
        public BrandRepository()
            : this(new AuthorityRepositoryContext())
        { 
        }

        public BrandRepository(IAuthorityRepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }

    }
}
