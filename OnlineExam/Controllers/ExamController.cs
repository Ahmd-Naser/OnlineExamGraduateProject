﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExam.Data;
using OnlineExam.Models;
using OnlineExam.Repository.ExamRepos;
using OnlineExam.Repository.QuestionsRepo;
using OnlineExam.ViewModels;
using System.Security.Claims;

namespace OnlineExam.Controllers
{
    [Authorize]
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExamRepo _examContext;
        private readonly IQuestionRepo _questionContext;

        // private string ConnectionString = "Server=(localdb)\\ProjectModels;Database=OnlineExam;Trusted_Connection=True;MultipleActiveResultSets=true";
        public ExamController(ApplicationDbContext context, IExamRepo examContext, IQuestionRepo questionContext)
        {
            _context = context;
            this._examContext = examContext;
            this._questionContext = questionContext;
        }


        public IActionResult Index()
        {

            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var Examlist = _examContext.GetAllExams(userId);


            return View(Examlist);
        }

        [HttpGet]
        public IActionResult CreateExam()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateExam(Exam item)
        {
            _examContext.CreateExam(item, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return RedirectToAction("Index");
        }

        public IActionResult EditExam(int Id)
        {

            var item = _examContext.FindExam(Id);

            if (item == null)
            {
                return NotFound();
            }


            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditExam(Exam item)
        {
            _examContext.EditExam(item.ExamId, item);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }

            _examContext.RemoveExam(Id);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ViewQuestions(int Id)
        {
            var exam = _examContext.FindExam(Id);
            if(exam is not null)
            {
                var examwithquestions = _questionContext.listexq(Id);
                return View(examwithquestions);
            }

            return NotFound();
        }

        [HttpGet]
        public IActionResult CreateQuestion(int Id)
        {
            var question = new Question();

            var exam = _examContext.FindExam(Id);

            if(exam is null)
            {
                return NotFound();
            }

            question.ExamId = Id;
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateQuestion(Question item)
        {

            int? id = item.ExamId;

            _context.Questions.Add(item);
            _context.SaveChanges();

            return RedirectToAction("ViewQuestions", new { Id = id });
        }

        //[HttpPost]
        public IActionResult DeleteQuestion(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }

            var item = _context.Questions.Find(Id);

            int? id = item.ExamId;

            _context.Questions.Remove(item);
            _context.SaveChanges();


            return RedirectToAction("ViewQuestions", new { Id = id });
        }



        public IActionResult EditQuestion(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }


            var item = _context.Questions.Find(Id);

            if (item == null)
            {
                return NotFound();
            }


            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditQuestion(Question item)
        {
            // var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            //item.ApplicationUserId = userId;

            _context.Questions.Update(item);

            _context.SaveChanges();

            return RedirectToAction("ViewQuestions", new { Id = item.ExamId });
        }


        public IActionResult Dashboard()
        {
            List<DashboardViewModel> model = new List<DashboardViewModel>();

            foreach (var i in _context.Answers)
            {
                DashboardViewModel item = new DashboardViewModel
                {
                    StudentId = i.StudentNationalId,
                    StudentName = i.StudentName,
                    Score = (int)i.Score,
                    ExamName = _context.Exams.First(j => j.ExamId == i.ExamId).Title

                };

                model.Add(item);

            }


            return View(model);
        }

        public IActionResult ViewResults(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            ResultsModel model = new ResultsModel();
            model.ExamName = _context.Exams.Where(i => i.ExamId == id).ToArray()[0].Title;

            model.answers = _context.Answers.Where(i => i.ExamId == id).ToList();

            int total = model.answers.Count;
            int numOFQuestions = _context.Questions.Where(i => i.ExamId == id).Count();
            int critical = numOFQuestions / 2;
            int numOfSucceeded = 0, sumOfScores = 0;
            model.isStudentPassed = new List<bool>();
            model.StudentsStatus = new List<String>();

            foreach (var i in model.answers)
            {
                if (i.Score >= critical)
                {
                    numOfSucceeded++;
                    model.isStudentPassed.Add(true);

                }
                else
                {
                    model.isStudentPassed.Add(false);
                }
                model.StudentsStatus.Add(GetLetterGrade((int)i.Score, total));
                sumOfScores += (int)i.Score;

            }

            model.numberOfPassedStudent = numOfSucceeded;
            model.numberOfFailedStudent = total - numOfSucceeded;
            total = Math.Max(total, 1);
            model.averageGrade = sumOfScores / total;

            return View(model);
        }
        public string GetLetterGrade(int totalScore, int totalQuestions)
        {
            double scorePercent = ((double)totalScore / totalQuestions) * 100;
            string letterGrade;

            if (scorePercent >= 90)
            {
                letterGrade = "A+";
            }
            else if (scorePercent >= 85)
            {
                letterGrade = "A";
            }
            else if (scorePercent >= 80)
            {
                letterGrade = "A-";
            }
            else if (scorePercent >= 75)
            {
                letterGrade = "B+";
            }
            else if (scorePercent >= 70)
            {
                letterGrade = "B";
            }
            else if (scorePercent >= 65)
            {
                letterGrade = "B-";
            }
            else if (scorePercent >= 60)
            {
                letterGrade = "C+";
            }
            else if (scorePercent >= 55)
            {
                letterGrade = "C";
            }
            else if (scorePercent >= 50)
            {
                letterGrade = "C-";
            }
            else if (scorePercent >= 45)
            {
                letterGrade = "D+";
            }
            else if (scorePercent >= 40)
            {
                letterGrade = "D";
            }
            else
            {
                letterGrade = "F";
            }

            return letterGrade;
        }
        public IActionResult ExamResultDetails(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            ExamAnswerResult model = new ExamAnswerResult();

            Answer ans = _context.Answers.Where(i => i.AnswerId == id).ToList()[0];
            model.StudentName = ans.StudentName;
            int ExamId = _context.Answers.Where(i => i.AnswerId == id).ToList()[0].ExamId;
            model.ExamName = _context.Exams.Where(i => i.ExamId == ExamId).ToList()[0].Title;

            model.lstOfQuestionAnswers = _context.AnswerQuestions.Where(i => i.AnswerId == id).ToList();

            model.numOfQuestions = _context.Questions.Where(i => i.ExamId == ExamId).Count();

            int score = 0;

            foreach (var i in model.lstOfQuestionAnswers)
            {
                if (i.TrueAnswer == i.SelectedAnswer)
                    score++;


            }

            model.score = score;

            if (score * 2 >= model.numOfQuestions)
                model.passed = true;




            return View(model);
        }



    }
}
