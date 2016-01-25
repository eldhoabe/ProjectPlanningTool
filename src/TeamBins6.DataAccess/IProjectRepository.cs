﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using TeamBins.Common;
using Dapper;
using TeamBins.Common.ViewModels;

namespace TeamBins.DataAccess
{
    public class BaseRepo
    {
        protected string ConnectionString
        {
            get { return "Data Source=DET-4082;Initial Catalog=Team;Integrated Security=true"; }
        }
    }
    public interface IProjectRepository
    {
        IEnumerable<ProjectDto> GetProjects(int teamId);
        bool DoesProjectsExist(int teamId);
        void Save(CreateProjectVM model);
    }

    public class ProjectRepository : BaseRepo,IProjectRepository
    {
        public IEnumerable<ProjectDto> GetProjects(int teamId)
        {
            throw new NotImplementedException();
        }

        public bool DoesProjectsExist(int teamId)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                con.Open();
                var projectCount = con.Query<int>("SELECT COUNT(1) FROM Project WHERE TeamId=@teamId", new {@teamId = teamId});
                return projectCount.First() > 0;
            }
          
        }

        public void Save(CreateProjectVM model)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                con.Open();
                var projectCount = con.Query<int>("INSERT INTO Project(Name,TeamID,CreatedDate,CreatedByID) VALUES (@name,@teamId,@dt,@createdById)",
                    new { @name=model.Name, @teamId = model.TeamId,@dt=DateTime.Now, @createdById =model.CreatedById});

                
            }
        }
    }


    //public class ProjectRepository : IProjectRepository
    //{
    //    private readonly TeamEntitiesConn db;
    //    public ProjectRepository()
    //    {
    //        db = new TeamEntitiesConn();
    //    }

    //    public bool DoesProjectsExist(int teamId)
    //    {
    //        return db.Projects.Any(s => s.TeamID == teamId);
    //    }

    //    public IEnumerable<ProjectDto> GetProjects(int teamId)
    //    {
    //        var projectList = new List<TeamBins.Common.ProjectDto>();
            
    //        using (var c = new SqlConnection(db.Database.Connection.ConnectionString))
    //        {

    //            var cmd = new SqlCommand("SELECT ID,Name from Project where TeamId=@teamId",c);
    //            cmd.Parameters.AddWithValue("teamId", teamId);
    //            c.Open();
    //            var reader = cmd.ExecuteReader();
    //            if (reader.HasRows)
    //            {
    //                while (reader.Read())
    //                {
    //                    var p = new ProjectDto
    //                    {
    //                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
    //                        Name = reader.GetString(reader.GetOrdinal("Name"))
    //                    };
    //                    projectList.Add(p);
    //                }
    //            }
    //        }

    //        return projectList;
    //    }
    //}
}