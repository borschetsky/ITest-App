﻿using System.Collections.Generic;
using ITestApp.Common.Providers;
using ITestApp.Data.Models;
using ITestApp.Data.Repository;
using ITestApp.Data.Saver;
using ITestApp.DTO;
using ITestApp.Services.Contracts;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace ITestApp.Services
{
    public class TestsService : ITestsService
    {
        private readonly ISaver saver;
        private readonly IMappingProvider mapper;
        private readonly IRepository<Test> tests;
        private readonly IRepository<Question> questions;
        private readonly IRepository<Answer> answers;
        private readonly IRepository<Category> categories;
        private readonly IRepository<UserTest> userTests;

        public TestsService(IRepository<UserTest> userTests, ISaver saver, IMappingProvider mapper, IRepository<Test> tests, IRepository<Question> questions, IRepository<Answer> answers, IRepository<Category> categories)
        {
            this.saver = saver ?? throw new ArgumentNullException("Saver can not be null");
            this.mapper = mapper ?? throw new ArgumentNullException("Mapper can not be null");
            this.tests = tests ?? throw new ArgumentNullException("Tests repo can not be null");
            this.answers = answers ?? throw new ArgumentNullException("Answers repo can not be null");
            this.questions = questions ?? throw new ArgumentNullException("Questions repo can not be null");
            this.categories = categories ?? throw new ArgumentNullException("Categories repo can not be null");
            this.userTests = userTests;
        }

        public void Delete(int id)
        {
            var testToDelete = tests.All.Include(q => q.Questions).ThenInclude(a => a.Answers)
                .FirstOrDefault(t => t.Id == id) ?? throw new ArgumentNullException("Test can not be null");

            tests.Delete(testToDelete);

            foreach (var question in testToDelete.Questions)
            {
                this.questions.Delete(question);
                foreach (var answer in question.Answers)
                {
                    this.answers.Delete(answer);
                }
            }

            saver.SaveChanges();
        }

        public void Edit(TestDto test)
        {
            Test testToEdit = tests.All.Where(t => t.Id == test.Id)
                .Include(q => q.Questions)
                .ThenInclude(a => a.Answers).FirstOrDefault() ?? throw new ArgumentNullException("Test can not be null.");

            testToEdit.Title = test.Title;
            //testToEdit.CategoryId = test.CategoryId;
            testToEdit.RequiredTime = test.RequiredTime;

            tests.Update(testToEdit);
            saver.SaveChanges();
        }

        public IEnumerable<QuestionDto> GetQuestions(int testId)
        {
            var currTests = tests.All
                .Where(t => t.Id == testId)
                .Select(q => q.Questions) ?? throw new ArgumentNullException("Collection of Questions can not be null");

            var result = mapper.ProjectTo<QuestionDto>(currTests);

            return result;
        }

        public void Publish(TestDto test)
        {
            if (test == null)
            {
                throw new ArgumentNullException("Test cannot be null!");
            }

            var testToFind = tests.All.FirstOrDefault(t => t.Id == test.Id);

            if (testToFind == null) //The test can be new and directry published or can be existing in the DB and only needs to change status. 
            {
                test.StatusId = 1; //Publish
                var newPublishedTest = mapper.MapTo<Test>(test);
                tests.Add(newPublishedTest);
            }
            else
            {
                testToFind.StatusId = 2; //Draft
                tests.Update(testToFind);
            }

            saver.SaveChanges();
        }

        public void SaveAsDraft(TestDto test)
        {
            test.StatusId = 2; //Draft
            tests.Add(mapper.MapTo<Test>(test));
            saver.SaveChanges();

        }

        public TestDto GetById(int id)
        {
            Test testWithId = tests.All.Where(t => t.Id == id)
                .Include(t => t.Status)
                .Include(t => t.Category)
                .Include(t => t.Author)
                .Include(q => q.Questions)
                    .ThenInclude(a => a.Answers)
                .FirstOrDefault() ?? throw new ArgumentNullException("Test can not be null");
            return mapper.MapTo<TestDto>(testWithId);
        }

        public IEnumerable<TestDto> GetByAuthorId(string id)
        {
            var currentTests = tests.All.
                Where(test => test.AuthorId == id)
                .Include(q => q.Questions).ThenInclude(a => a.Answers);

            return mapper.ProjectTo<TestDto>(currentTests);
        }

        public IEnumerable<TestDto> GetAllTests()
        {
            var allTests = tests.All
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers);

            return mapper.ProjectTo<TestDto>(allTests);
        }

        /// <summary>
        /// Saves a newly created test to the database.
        /// </summary>
        /// <param name="test">DTO test to be saved.</param>
        public void SaveToDb(TestDto test)
        {
            var newTestEntity = mapper.MapTo<Test>(test) ?? throw new ArgumentNullException("Test Can Not Be Null");
            tests.Add(newTestEntity);
            saver.SaveChanges();
        }

        /// <summary>
        /// Gets a new test, converts it to DB entity, saves it to the database and returns the newly created entity test as DTO.
        /// </summary>
        /// <param name="test">DTO test to be saved.</param>
        /// <returns>DTO test with id from the database.</returns>
        public TestDto CreateNew(TestDto test)
        {
            var newTestEntity = mapper.MapTo<Test>(test) ?? throw new ArgumentNullException("Test Can Not Be Null");
            tests.Add(newTestEntity);
            saver.SaveChanges();

            return GetById(newTestEntity.Id);
        }

        public int GetTestDurationSeconds(int id)
        { 
            int seconds = tests.All.FirstOrDefault(t => t.Id == id).RequiredTime * 60;

            return seconds;
        }

        public string GetCategoryNameByTestId(int id)
        {
            var name = tests.All.
                Where(t => t.Id == id).Include(c => c.Category).
                FirstOrDefault().Category.Name;

            return name;
        }

        public int GetTestRequestedTime(int id)
        {
            int time = tests.All.FirstOrDefault(t => t.Id == id).RequiredTime;

            return time;
        }

        public string GetStatusNameByTestId(int id)
        {
            string name = tests.All
                .Where(t => t.Id == id).Include(st => st.Status)
                .FirstOrDefault().Status.Name ?? throw new ArgumentNullException("Status name cannot be null or empty");

            return name;
        }

        public void DisableTest(int id)
        {
            var test = tests.All.Where(t => t.Id == id && t.StatusId != 2).FirstOrDefault();
            if (test != null)
            {
                test.StatusId = 2; //Draft

                tests.Update(test);
                saver.SaveChanges();
            }

        }

        public void PublishExistingTest(int id)
        {
            var test = tests.All.Where(t => t.Id == id && t.StatusId != 1).FirstOrDefault();

            if (test != null)
            {
                test.StatusId = 1; //Published

                tests.Update(test);
                saver.SaveChanges();
            }

        }

        public TestDto GetRandomTestByCategory(string name, string user)
        {
            var filteredTests = tests.All.Include(c => c.Category).Include(ut => ut.UserTests).Where(t => t.StatusId != 2 && t.Category.Name == name);
            var userTest = userTests.All.Where(ut => ut.UserId == user && ut.TimeExpire > DateTime.Now.AddSeconds(5) && ut.SubmittedOn == null).FirstOrDefault();
            
            if (userTest != null)
            {
                var test = tests.All.FirstOrDefault(t => t.Id == userTest.TestId);
                var dto = mapper.MapTo<TestDto>(test);
                dto.TakinStatus = "Ongoing";
                return dto;
            }
            else if(filteredTests.Count() != 0)
            {
                var curenttests = new List<TestDto>();

                foreach (var item in filteredTests)
                {
                    if (!(item.UserTests.Any(tst => tst.UserId == user)))
                    {
                        curenttests.Add(mapper.MapTo<TestDto>(item));
                    }
                }
                if (curenttests.Count > 1)
                {
                    var randoninstance = new Random();
                    int testCount = curenttests.Count();
                    int random = randoninstance.Next(testCount);

                    var test = curenttests[random];
                    return test;
                }
                else
                {
                    return curenttests.FirstOrDefault();
                }
            }
            else
            {
                return null;
            }
        }
    }
}
