﻿using Dapper;
using Microsoft.Extensions.Options;
using powerful_crm.Core.Models;
using powerful_crm.Core.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace powerful_crm.Data
{
    public class AccountRepository : BaseRepository, IAccountRepository
    {
        public AccountRepository(IOptions<AppSettings> options) : base(options)
        {
            _connection = new SqlConnection(_connectionString);
        }
        public int AddAccount(AccountDto dto)
        {
            return _connection.QuerySingleOrDefault<int>(
                "dbo.Account_Add",
                param: new
                {
                    name = dto.Name,
                    currency = (int)dto.Currency,
                    leadId = dto.LeadDto.Id,
                },
                commandType: CommandType.StoredProcedure);
        }
        public int DeleteAccount(int id)
        {
            var result = _connection
                .Execute("dbo.Account_Delete",
                new { id },
                commandType: System.Data.CommandType.StoredProcedure);
            return result;
        }
        public AccountDto GetAccountById(int id)
        {
            return _connection.QueryFirstOrDefault<AccountDto>(
                "dbo.Account_SelectById",
                new { id },
                commandType: CommandType.StoredProcedure);
        }
        public List<AccountDto> GetAccountsByLeadId(int leadId)
        {
            return _connection.Query<AccountDto>(
                "dbo.Account_SelectByLeadId",
                new { leadId },
                commandType: System.Data.CommandType.StoredProcedure)
               .Distinct()
           .ToList();
        }
    }
}
