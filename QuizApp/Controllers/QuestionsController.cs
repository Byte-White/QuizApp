using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Questions
        public async Task<IActionResult> Index(int? categoryId)
        {
            IQueryable<Question> questions = _context.Questions.Include(q => q.Category);

            if (categoryId.HasValue)
            {
                questions = questions.Where(q => q.CategoryId == categoryId.Value);
            }

            return View(await questions.ToListAsync());
        }

        // GET: Questions/Search
        public async Task<IActionResult> Search()
        {
            return View();
        }

        // GET: Questions/SearchResult
        public async Task<IActionResult> SearchResult(string SearchPhrase)
        {
            var applicationDbContext = _context.Questions.Include(q => q.Category);
            return View("Index", await applicationDbContext.Where(x=>x.Text.Contains(SearchPhrase)).ToListAsync());
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Category)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");  // Show Category Name
            return View();
        }

        // POST: Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Text,State,CategoryId,Answers")] Question question)
        {
            _context.Add(question);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            // throw new Exception($"DEBUG: Could not create a question [{question.Id} {question.Text} {question.State} {question.CategoryId}]");

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", question.CategoryId);
            return View(question);  // Return to the view with error messages
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                                          .Include(q => q.Category)
                                          .Include(q => q.Answers)
                                          .FirstOrDefaultAsync(m => m.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            // Populate categories in ViewBag for dropdown in the view
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", question.CategoryId);

            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Text,State,CategoryId,Answers")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            try
            {
                var existingQuestion = await _context.Questions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (existingQuestion == null)
                {
                    return NotFound();
                }

                existingQuestion.Text = question.Text;
                existingQuestion.State = question.State;
                existingQuestion.CategoryId = question.CategoryId;

                var answersToRemove = existingQuestion.Answers
                    .Where(a => !question.Answers.Any(newA => newA.Id == a.Id))
                    .ToList();

                _context.Answers.RemoveRange(answersToRemove);

                foreach (var answer in question.Answers)
                {
                    if (answer.Id == 0) // New Answer
                    {
                        answer.QuestionId = existingQuestion.Id;
                        _context.Answers.Add(answer);
                    }
                    else
                    {
                        var existingAnswer = existingQuestion.Answers.FirstOrDefault(a => a.Id == answer.Id);
                        if (existingAnswer != null)
                        {
                            existingAnswer.Text = answer.Text;
                            existingAnswer.IsCorrect = answer.IsCorrect;
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(question.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }




        // GET: Questions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}
