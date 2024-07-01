using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using 南岩ERP.Models;
using 南岩ERP.Models.TableTest;
using 南岩ERP.TableTest;

namespace 南岩ERP.Controllers
{
    public class TableTestController(nanotableContext db) : Controller
    {
        private readonly nanotableContext db = db;
        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await db.Table_1.ToListAsync();
                if (data == null || !data.Any())
                {
                    // Log a message if data is null or empty
                    Console.WriteLine("No data found in the table.");
                }
                return View(data);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception: {ex.Message}");
                // Return an error view or message
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
        public async Task<IActionResult> Edit(int id)
        {
            var table = await db.Table_1.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }
            return PartialView("_EditPartial", table);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Table_1 t1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var originalt1 = await db.Table_1.Where(m => m.id == id).AsNoTracking().FirstOrDefaultAsync();
            if (originalt1 == null)
            {
                return NotFound(ModelState);
            }

            try
            {
                db.Entry(t1).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Ok(t1);
            }
            catch (DbUpdateConcurrencyException DBex)
            {
                // 处理并发冲突
                Console.WriteLine($"Concurrency Exception: {DBex.Message}");

                // 获取引起冲突的实体项
                var entry = DBex.Entries.Single();
                var databaseValues = entry.GetDatabaseValues();
                var clientValues = entry.Entity;

                // 判断哪些属性发生了变化
                if (databaseValues == null)
                {
                    Console.WriteLine("Unable to save changes. The entity was deleted by another user.");
                }
                else
                {
                    Console.WriteLine("The record you attempted to edit was modified by another user after you got the original value. Your edit operation was canceled and the current values in the database have been displayed. If you still want to edit this record, click the Save button again.");
                }
                return Conflict(new { message = DBex.Message });
            }

        }

        public IActionResult Index2()
        {
            var t1 = db.Table_1.ToList();
            return View(t1);
        }
        public IActionResult Edit2(int id)
        {
            var table = db.Table_1.Find(id);
            if (table == null)
            {
                return NotFound();
            }
            return PartialView("_EditPartial", table);
        }
        [HttpPost]
        public IActionResult Edit2(int id, Table_1 t1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var originalt1 = db.Table_1.FirstOrDefault(m => m.id == id);
            if (originalt1 == null)
            {
                return NotFound(ModelState);
            }
            originalt1.name1 = t1.name1;
            originalt1.name = t1.name;
            db.Table_1.Update(originalt1);

            try
            {
                db.SaveChanges();
                return Ok(t1);
            }
            catch (DbUpdateConcurrencyException DBex)
            {
                // 处理并发冲突
                Console.WriteLine($"Concurrency Exception: {DBex.Message}");
                return Conflict(new { message = DBex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }

        }
        public IActionResult test1()
        {
            Table_1 t1 = new Table_1()
            {
                name = "name",
                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            Table_2 t2 = new Table_2()
            {

                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            Table_3 t3 = new Table_3()
            {
                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            Table_4 t4 = new Table_4()
            {
                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            Table_5 t5 = new Table_5()
            {
                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            Table_1 t6 = new Table_1()
            {
                name = "name",
                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            Table_1 t7 = new Table_1()
            {
                name = "name",
                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            Table_1 t8 = new Table_1()
            {
                name = "name",
                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            Table_1 t9 = new Table_1()
            {
                name = "name",
                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            Table_1 t10 = new Table_1()
            {
                name = "name",
                name1 = "name",
                name2 = "name",
                name3 = "name",
                name4 = "name",
            };
            db.Table_1.Add(t1);
            db.Table_2.Add(t2);
            db.Table_3.Add(t3);
            db.Table_4.Add(t4);
            db.Table_5.Add(t5);
            db.Table_1.Add(t6);
            db.Table_1.Add(t7);
            db.Table_1.Add(t8);
            db.Table_1.Add(t9);
            db.Table_1.Add(t10);
            db.Table_1.Add(t1);

            db.SaveChanges();

            return RedirectToAction("index");
        }
        public IActionResult test2(string name)
        {
            var t1 = db.Table_1.Where(m => m.id == 1).FirstOrDefault();
            t1.name1 = name;
            db.SaveChanges();
            return RedirectToAction("index");

        }
        public IActionResult test3()
        {
            return RedirectToAction("index");
        }

        public IActionResult test4(int id)
        {
            var table = new TableTestView()
            {
                Table_1 = db.Table_1.FirstOrDefault(m => m.id == id),
                Table_1_preview = db.Table_1.FirstOrDefault(m => m.id == id)
            };
            return View(table);
        }


        [HttpPost]
        public IActionResult test4(TableTestView table,int id)
        {
            var table_t1 = db.Table_1.FirstOrDefault(m => m.id ==id);
            var table_t1_pr = table.Table_1_preview;
            if (table_t1 == null || table_t1_pr == null)
            {
                return NotFound(new { message = "Record not found." });
            }
            bool bobo = AreTablesEqual(table_t1_pr, table_t1);
            if(!bobo)
            {
                Console.WriteLine("資料已被修改");
                return Conflict(new { message = "資料已被修改" });
            }
            else
            {
                var t1 = table.Table_1;
                try
                {
                    table_t1.name = t1.name;
                    table_t1.name1 = t1.name1;
                    table_t1.name2 = t1.name2;
                    table_t1.name3 = t1.name3;
                    table_t1.name4 = t1.name4;
                    db.Table_1.Update(table_t1);
                    db.SaveChanges();
                    Console.WriteLine("資料修改完成");
                    return Ok(new { message = "資料修改完成" });
                }
                catch(Exception ex) { 
                    Console.WriteLine(ex.ToString());
                    return StatusCode(500, new { message = ex.Message });

                }
            }
        }

        public bool AreTablesEqual(Table_1 table1, Table_1 table2)
        {
            if (table1 == null || table2 == null)
                return false;

            return table1.id == table2.id &&
                   table1.name == table2.name &&
                   table1.name1 == table2.name1 &&
                   table1.name2 == table2.name2 &&
                   table1.name3 == table2.name3 &&
                   table1.name4 == table2.name4;
            // Compare other properties as needed
        }
    }
}
