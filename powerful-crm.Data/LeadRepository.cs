using System;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Options;
using powerful_crm.Core.Settings;
using System.Data;
using powerful_crm.Core.Models;
using System.Linq;
using System.Collections.Generic;

namespace powerful_crm.Data
{
    public class LeadRepository : BaseRepository, ILeadRepository
    {
        public LeadRepository(IOptions<AppSettings> options) : base(options)
        {
            _connection = new SqlConnection(_connectionString);
        }

        public int AddLead(LeadDto dto)
        {
            return _connection.QuerySingle<int>(
                "dbo.Lead_AddUpdate",
                param: new
                {
                    dto.FirstName,
                    dto.LastName,
                    dto.Login,
                    dto.Password,
                    dto.Email,
                    dto.Phone,
                    CityId = dto.City.Id,
                    dto.BirthDate
                },
                commandType: CommandType.StoredProcedure);
        }

        public int UpdateLead(LeadDto dto)
        {
            return _connection.QuerySingle<int>(
                "dbo.Lead_AddUpdate",
                param: new
                {
                    dto.Id,
                    dto.FirstName,
                    dto.LastName,
                    dto.Email,
                    dto.Phone,
                    CityId = dto.City.Id,
                    dto.BirthDate
                },
                commandType: CommandType.StoredProcedure);
        }
        public int DeleteOrRecoverLead(int id, bool isDeleted)
        {
            return _connection
                .Execute("dbo.Lead_DeleteOrRecover",
                new
                {
                    id,
                    isDeleted
                },
                commandType: CommandType.StoredProcedure);
        }

        public int ChangePasswordLead(int id, string oldPassword, string newPassword)
        {
            return _connection
               .Execute("dbo.Lead_ChangePassword", new
               {
                   id,
                   oldPassword,
                   newPassword
               },
               commandType: CommandType.StoredProcedure);
        }

        public LeadDto GetLeadById(int id)
        {
            return _connection.Query<LeadDto, CityDto, LeadDto>(
                "dbo.Lead_SelectById", (lead, city) =>
                {
                    lead.City = city;
                    return lead;
                },
                new { id },
                splitOn: "Id",
                commandType: CommandType.StoredProcedure).FirstOrDefault();
        }
        public List<LeadDto> SearchLeads(LeadDto leadDto)
        {
            if (leadDto.FirstName == null && leadDto.LastName == null && leadDto.Email == null && leadDto.Login == null
                && leadDto.Phone == null && leadDto.BirthDate == null && leadDto.City == null)
                throw new ArgumentNullException();
            return _connection.Query<LeadDto, CityDto, LeadDto>(
                "dbo.Lead_Search", (lead, city) =>
                {
                    lead.City = city;
                    return lead;
                },
                    new
                    {
                        leadDto.FirstName,
                        leadDto.LastName,
                        leadDto.Email,
                        leadDto.Login,
                        leadDto.Phone,
                        leadDto.BirthDate,
                        @�ityName = leadDto.City.Name
                    },
                    splitOn: "Id",
                    commandType: System.Data.CommandType.StoredProcedure)
                .Distinct()
                .ToList();
        }
        public int AddCity(CityDto dto)
        {
            return _connection.QuerySingle<int>(
                "dbo.City_Add",
                param: new
                {
                    dto.Name
                },
                commandType: CommandType.StoredProcedure);
        }
        public int DeleteCity(int id)
        {
            return _connection.Execute(
                "dbo.City_Delete",
                param: new
                {
                    id
                },
                commandType: CommandType.StoredProcedure);
        }


    }
}
