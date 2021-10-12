using Intellegens.Commons.DemoApi.Dto;
using Intellegens.Commons.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Intellegens.Commons.DemoApi.Tests
{
    public class StudentsControllerTests : TestsBase<StudentDto>
    {
        public StudentsControllerTests() : base("/api/v1/students")
        {
        }

        [Fact]
        public async Task CreatingStudentShouldWork()
        {
            var student = GetStudent();

            var data = await Create(student);

            Assert.True(data.Success);

            var studentCreated = data.ResponseData.Data.First();

            Assert.Equal(student.FullName, studentCreated.FullName);
            Assert.Equal(student.DateOfBirth, studentCreated.DateOfBirth);
            Assert.NotEqual(student.Id, studentCreated.Id);
            Assert.NotEqual(student.PersonId, studentCreated.PersonId);
        }

        [Fact]
        public async Task CreatingStudentWithoutProvidingDataShouldReturn400()
        {
            var student = GetStudent();
            student.FullName = null;

            var data = await Create(student);

            Assert.False(data.Success);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task FetchingNonExistingStudentShouldReturn404()
        {
            var data = await Get(Int32.MaxValue);
            Assert.Equal(404, data.StatusCode);
        }

        [Fact]
        public async Task FetchingStudentShouldWork()
        {
            var student = await CreateStudent();

            var data = await Get(student.Id);

            Assert.True(data.Success);

            var studentFetched = data.ResponseData.Data.First();

            Assert.Equal(student.FullName, studentFetched.FullName);
            Assert.Equal(student.Id, studentFetched.Id);
            Assert.Equal(student.PersonId, studentFetched.PersonId);
            Assert.Equal(student.DateOfBirth, studentFetched.DateOfBirth);
        }

        [Fact]
        public async Task SearchingStudentsByNameShouldWork()
        {
            var student1 = await CreateStudent();
            var student2 = await CreateStudent();
            var student3 = await CreateStudent();

            var searchRequest = new SearchRequest
            {
                Values = new List<string> { student1.FullName, student2.FullName, student3.FullName },
                ValuesLogic = LogicOperators.ANY,
                Operator = Operators.EQUALS,
                Keys = new List<string> { nameof(StudentDto.FullName) }
            };

            var response = await Search(searchRequest);

            Assert.True(response.Success);

            var originalIds = new List<int> { student1.Id, student2.Id, student3.Id };
            var studentsSearchIds = response.ResponseData.Data.Select(x => x.Id);
            Assert.Equal(originalIds.Count, originalIds.Intersect(studentsSearchIds).Count());
            Assert.Equal(originalIds.Count, studentsSearchIds.Count());
        }

        [Fact]
        public async Task SearchingStudentsByPersonIdShouldWork()
        {
            var student1 = await CreateStudent();
            var student2 = await CreateStudent();
            var student3 = await CreateStudent();

            var searchRequest = new SearchRequest
            {
                Values = new List<string> { student1.PersonId.ToString(), student2.PersonId.ToString(), student3.PersonId.ToString() },
                ValuesLogic = LogicOperators.ANY,
                Operator = Operators.EQUALS,
                Keys = new List<string> { nameof(StudentDto.PersonId) }
            };

            var response = await Search(searchRequest);

            Assert.True(response.Success);

            var originalIds = new List<int> { student1.Id, student2.Id, student3.Id };
            var studentsSearchIds = response.ResponseData.Data.Select(x => x.Id);
            Assert.Equal(originalIds.Count, originalIds.Intersect(studentsSearchIds).Count());
            Assert.Equal(originalIds.Count, studentsSearchIds.Count());
        }

        [Fact]
        public async Task SearchingStudentsByIdShouldWork()
        {
            var student1 = await CreateStudent();
            var student2 = await CreateStudent();
            var student3 = await CreateStudent();

            var searchRequest = new SearchRequest
            {
                Values = new List<string> { student1.Id.ToString(), student2.Id.ToString(), student3.Id.ToString() },
                ValuesLogic = LogicOperators.ANY,
                Operator = Operators.EQUALS,
                Keys = new List<string> { nameof(StudentDto.Id) }
            };

            var response = await Search(searchRequest);

            Assert.True(response.Success);

            var originalIds = new List<int> { student1.Id, student2.Id, student3.Id };
            var studentsSearchIds = response.ResponseData.Data.Select(x => x.Id);
            Assert.Equal(originalIds.Count, originalIds.Intersect(studentsSearchIds).Count());
            Assert.Equal(originalIds.Count, studentsSearchIds.Count());
        }

        [Fact]
        public async Task UpdatingStudentShouldntWorkForIdMismatch()
        {
            var student = await CreateStudent();
            var data = await Update(student.Id + 10, student);

            Assert.False(data.Success);
        }

        [Fact]
        public async Task UpdatingStudentShouldWork()
        {
            var student = await CreateStudent();
            student.DateOfBirth = student.DateOfBirth.AddDays(-2);
            student.FullName += "_updated";

            var data = await Update(student.Id, student);

            Assert.True(data.Success);

            var studentUpdated = data.ResponseData.Data.First();

            Assert.Equal(student.FullName, studentUpdated.FullName);
            Assert.Equal(student.DateOfBirth, studentUpdated.DateOfBirth);
            Assert.Equal(student.Id, studentUpdated.Id);
            Assert.Equal(student.PersonId, studentUpdated.PersonId);
        }

        [Fact]
        public async Task UpdatingStudentWithoutRequiredDataShouldReturn400()
        {
            var student = await CreateStudent();
            student.FullName = null;

            var data = await Update(student.Id, student);

            Assert.False(data.Success);
            Assert.Equal(400, data.StatusCode);
        }

        private async Task<StudentDto> CreateStudent(string fullname = null, DateTime? dateOfBirth = null)
        {
            var data = await Create(GetStudent(fullname, dateOfBirth));
            return data.ResponseData.Data.First();
        }

        private StudentDto GetStudent(string fullname = null, DateTime? dateOfBirth = null)
        {
            return new StudentDto
            {
                FullName = fullname ?? Guid.NewGuid().ToString(),
                DateOfBirth = dateOfBirth ?? DateTime.Now.AddYears(-1 * random.Next(18, 60)).Date
            };
        }
    }
}