using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit8.Students.Common;
using Bit8.Students.Services.Students;
using Dapper;

namespace Bit8.Students.Query.Students
{
    public class StudentQuery : QueryBase, IStudentQuery
    {
        public StudentQuery(IBConfiguration configuration) : base(configuration)
        {
        }

        public async Task<IEnumerable<GetTopTenStudentsQuery>> GetTopTenAsync()
        {
            var sql = @"select s.name, avg(sa.score) as average_score from student s
                        left join student_assignment sa on s.id = sa.student_id
                        group by s.name
                        order by average_score desc
                        limit 10;";

            var result = await Connection.QueryAsync<GetTopTenStudentsQuery>(sql);
            return result;
        }

        public async Task<IEnumerable<GetDisciplinesWithoutScoreQuery>> GetDisciplinesWithoutScore()
        {
            var sql = @"select without_scores.sname student_name, without_scores.dname discipline_name from (
                            select distinct s.id sid, d.id did from student s
                            right join student_assignment sa on s.id = sa.student_id
                            right join discipline_semester ds on sa.discipline_semester_id = ds.id
                            right join discipline d on ds.discipline_id = d.id
                            where sa.score is not null) as with_scores
                        right join (
                                select s.id sid, d.id did, s.name sname, d.name dname from student s
                                cross join discipline d
                                ) as without_scores on without_scores.sid = with_scores.sid and without_scores.did = with_scores.did
                        where with_scores.sid is null and with_scores.did is null";

            var result = await Connection.QueryAsync(sql, 
                (string student, string discipline) => new {student, discipline}, 
                splitOn: "discipline_name");
            
            return result
                .GroupBy(x => x.student,
                    (key, group) => new GetDisciplinesWithoutScoreQuery
                    {
                        StudentName = key, 
                        DisciplineNames = group.Select(x => x.discipline)
                    });
        }

        public async Task<IEnumerable<GetAllWithSemestersQuery>> GetAllWithSemestersAsync()
        {
            var sql = @"select s.name student, sm.name semester from student s
                        join student_assignment sa on s.id = sa.student_id
                        join discipline_semester ds on sa.discipline_semester_id = ds.id
                        join semester sm on ds.semester_id = sm.id";

            var result = await Connection.QueryAsync(sql, (string student, string semester) => new {student, semester},
                splitOn: "semester");

            return result
                .GroupBy(x => x.student,
                    (key, group) => new GetAllWithSemestersQuery
                    {
                        StudentName = key,
                        SemesterNames = group.Select(x => x.semester)
                    });
        }
    }
}