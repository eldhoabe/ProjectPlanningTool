using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamBins.Common;
using TeamBins.Common.Infrastructure.Enums.TeamBins.Helpers.Enums;
using TeamBins.Common.ViewModels;
using TeamBins.DataAccess;
using TeamBins6.Common.Infrastructure.Exceptions;
using TeamBins6.Infrastrucutre.Extensions;
using TeamBins6.Infrastrucutre.Services;

namespace TeamBins.Services
{

    public class IssueManager : IIssueManager
    {
        private IActivityRepository activityRepository;
        private IIssueRepository issueRepository;
        private IProjectRepository iProjectRepository;
        private IUserSessionHelper userSessionHelper;
        public IssueManager(IIssueRepository issueRepository, IProjectRepository iProjectRepository, IActivityRepository activityRepository, IUserSessionHelper userSessionHelper)
        {
            this.issueRepository = issueRepository;
            this.iProjectRepository = iProjectRepository;
            this.activityRepository = activityRepository;
            this.userSessionHelper = userSessionHelper;
        }
        public DashBoardItemSummaryVM GetDashboardSummaryVM(int teamId)
        {
            throw new NotImplementedException();
        }



        public IssueDetailVM GetIssue(int id)
        {
            return this.issueRepository.GetIssue(id,this.userSessionHelper.UserId);
        }

        public IEnumerable<IssueVM> GetIssues(List<int> statusIds, int count)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IssueDetailVM>> GetIssuesAssignedToUser(int userId)
        {
            return await this.issueRepository.GetIssuesAssignedToUser(userId);
        }
        public IEnumerable<IssuesPerStatusGroup> GetIssuesGroupedByStatusGroup(int teamId, int count)
        {
            return this.issueRepository.GetIssuesGroupedByStatusGroup(count,teamId,this.userSessionHelper.UserId);
        }

        public ActivityDto SaveActivity(CreateIssue model, IssueDetailVM previousVersion, IssueDetailVM newVersion)
        {
            bool isStateChanged = false;
            var activity = new ActivityDto() { ObjectId = newVersion.Id, ObjectType = "Issue" };

            if (previousVersion == null)
            {
                activity.ObjectUrl = "issue/" + newVersion.Id;
                //activity.CreatedBy = 
                activity.Description = "Created";
                activity.ObjectTitle = model.Title;
                isStateChanged = true;
            }
            else
            {
                if (previousVersion.Status.Id != newVersion.Status.Id)
                {
                    // status of issue updated
                    activity.Description = "Changed status";
                    activity.NewState = newVersion.Status.Name;
                    activity.OldState = previousVersion.Status.Name;
                    activity.ObjectTitle = newVersion.Title;
                    isStateChanged = true;
                }
                else if (previousVersion.Category.Id != newVersion.Category.Id)
                {
                    activity.Description = "Changed category";
                    activity.NewState = newVersion.Category.Name;
                    activity.OldState = previousVersion.Category.Name;
                    activity.ObjectTitle = newVersion.Title;
                    isStateChanged = true;
                }


            }
            activity.CreatedTime = DateTime.Now;
            activity.TeamId = userSessionHelper.TeamId;

            activity.Actor = new UserDto { Id = userSessionHelper.UserId };

            if (isStateChanged)
            {
                var newId = activityRepository.Save(activity);
                return activityRepository.GetActivityItem(newId);

            }
            return null;
        }


        public void LoadDropdownData(CreateIssue issue)
        {
            issue.Projects =
                   this.iProjectRepository.GetProjects(this.userSessionHelper.TeamId)
                       .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                       .ToList();

         issue.Statuses = this.issueRepository.GetStatuses()
                  .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                       .ToList();

            issue.Priorities = this.issueRepository.GetPriorities()
                      .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                           .ToList();

            issue.Categories = this.issueRepository.GetCategories()
          .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
               .ToList();



        }

        public async Task<IssueDetailVM> SaveIssue(CreateIssue issue, List<IFormFile> files)
        {
            if (issue.SelectedProject == 0)
            {
                var defaultProject = await this.iProjectRepository.GetDefaultProjectForTeamMember(this.userSessionHelper.TeamId,
                     this.userSessionHelper.UserId);
                if (defaultProject==null)                
                {
                    throw new MissingSettingsException("Missing data", "Default project");
                }
                issue.SelectedProject = defaultProject.Id;

            }
            if (issue.SelectedCategory == 0)
            {
                var categories = this.issueRepository.GetCategories();
                issue.SelectedCategory = categories.First().Id;
            }
            if (issue.SelectedPriority == 0)
            {
                var categories = this.issueRepository.GetPriorities();
                issue.SelectedPriority = categories.First().Id;
            }
            if (issue.SelectedStatus == 0)
            {
                var statuses = this.issueRepository.GetStatuses();
                issue.SelectedStatus = statuses.First().Id;
            }
            issue.CreatedByID = this.userSessionHelper.UserId;
            issue.TeamID = this.userSessionHelper.TeamId;
            var issueId = this.issueRepository.SaveIssue(issue);



            var issueDetail = this.issueRepository.GetIssue(issueId,this.userSessionHelper.UserId);
            return issueDetail;
            ;
        }

        public Task<int> StarIssue(int issueId)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            this.issueRepository.Delete(id, this.userSessionHelper.UserId);
        }
        public async Task<IEnumerable<UserDto>> GetNonIssueMembers(int issueId)
        {
            var members = await this.issueRepository.GetNonIssueMembers(issueId,this.userSessionHelper.TeamId);
            foreach (var userDto in members)
            {
                userDto.GravatarUrl = userDto.EmailAddress.ToGravatarUrl();
            }
            return members;
        }

        public async Task RemoveIssueMember(int issueId, int memberId)
        {
            await issueRepository.RemoveIssueMember(issueId, memberId);
        }

        public async Task SaveIssueAssignee(int issueId, int userId)
        {
            await issueRepository.SaveIssueMember(issueId, userId,userSessionHelper.UserId , IssueMemberRelationType.Assigned.ToString());
        }
        public async Task<IEnumerable<IssueMember>> GetIssueMembers(int issueId)
        {
            var members = await this.issueRepository.GetIssueMembers(issueId);
            members= members.Where(s => s.Relationtype == IssueMemberRelationType.Assigned.ToString()).ToList();
            foreach (var userDto in members)
            {
                userDto.Member.GravatarUrl = userDto.Member.EmailAddress.ToGravatarUrl();
            }
            return members;
        }

        public async Task StarIssue(int issueId, int userId, bool isRequestForToStar)
        {
            await this.issueRepository.StarIssue(issueId, this.userSessionHelper.UserId, isRequestForToStar);
        }

        public async Task SaveDueDate(int issueId, DateTime? dueDate)
        {
            await issueRepository.SaveDueDate(issueId, dueDate, this.userSessionHelper.UserId);
        }

    }
    public interface IIssueManager
    {
        Task<IEnumerable<IssueDetailVM>> GetIssuesAssignedToUser(int userId);
        Task RemoveIssueMember(int issueId, int memberId);
        Task<IEnumerable<IssueMember>> GetIssueMembers(int issueId);
        Task SaveIssueAssignee(int issueId, int userId);
        Task<IEnumerable<UserDto>> GetNonIssueMembers(int issueId);
        Task<int> StarIssue(int issueId);
        IEnumerable<IssueVM> GetIssues(List<int> statusIds, int count);
        Task<IssueDetailVM> SaveIssue(CreateIssue issue, List<IFormFile> files);
        DashBoardItemSummaryVM GetDashboardSummaryVM(int count);
        IssueDetailVM GetIssue(int id);
        ActivityDto SaveActivity(CreateIssue model, IssueDetailVM previousVersion, IssueDetailVM newVersion);

        IEnumerable<IssuesPerStatusGroup> GetIssuesGroupedByStatusGroup(int teamId, int count);
        void Delete(int id);
        void LoadDropdownData(CreateIssue issue);

        Task StarIssue(int issueId, int userId, bool isRequestForToStar);

        Task SaveDueDate(int issueId, DateTime? dueDate);

    }
}