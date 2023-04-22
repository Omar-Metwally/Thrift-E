using Data_Layer;
using Infrastructure_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Thrift_E.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly MaindbContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IUnitOfWork unitOfWork, MaindbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.MeasureOfScale)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    InstockQty = p.InstockQty,
                    Price = p.Price,
                    NewOrUsed = p.NewOrUsed,
                    SignupDate = p.SignupDate,
                    Image1 = p.Image1,
                    Image2 = p.Image2,
                    Image3 = p.Image3,
                    Image4 = p.Image4,
                    Description = p.Description,
                    CategoryName = p.Category.CategoryName,
                    MeasureOfScaleName = p.MeasureOfScale.MeasureOfScale
                })
                .ToList();

            return View(products);
        }

        public IActionResult Delete(int? id)
        {
            Product product = _context.Products.FirstOrDefault(x => x.ProductId == id);
            DeleteFile(product.Image1);
            DeleteFile(product.Image2);
            DeleteFile(product.Image3);
            DeleteFile(product.Image4);
            _unitOfWork.Products.Delete(id);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Upsert(int? id)
        {
            ViewBag.Categories = new SelectList(_context.Categorys.ToList(), "CategoryId", "CategoryName");
            ViewBag.MeasureOfScales = new SelectList(_context.MeasuresOfScales.ToList(), "MeasureOfScaleId", "MeasureOfScale");
            var entity = _unitOfWork.Products.Upsert(id);
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Upsert(int? id, Product product, IFormFile? file1, IFormFile? file2, IFormFile? file3, IFormFile? file4)
        {
            IFormFile[] files = { file1, file2, file3, file4 };
            string[] images = { product.Image1, product.Image2, product.Image3, product.Image4 };

            int x = 1;
            foreach (var (file, img) in files.Zip(images))
            {
                if (file != null) { SaveFile(file, product.ProductName, x, images); }
                x++;

            }
            product.Image1 = images[0];
            product.Image2 = images[1];
            product.Image3 = images[2];
            product.Image4 = images[3];
            if (id == null)
            {
                _context.Add(product);
                product.SignupDate = DateTime.Now;
            }
            else _context.Update(product);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        private void SaveFile(IFormFile file, string productName, int index, string[] images)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            var uploads = Path.Combine(wwwRootPath, "Images", "Products");
            var extension = Path.GetExtension(file.FileName);
            var filename = $"{productName} {index}{extension}";

            var filePath = Path.Combine(uploads, filename);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            images[index - 1] = @"\Images\Products\" + filename;
        }

        private void DeleteFile(string filePath)
        {
            string oldimage;
            if (filePath != null)
            {
                oldimage = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('\\'));



                System.IO.File.Delete(oldimage);
            }

        }

        public IActionResult Details(int? id)
        {

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.MeasureOfScale)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    InstockQty = p.InstockQty,
                    Price = p.Price,
                    NewOrUsed = p.NewOrUsed,
                    SignupDate = p.SignupDate,
                    Image1 = p.Image1,
                    Image2 = p.Image2,
                    Image3 = p.Image3,
                    Image4 = p.Image4,
                    Description = p.Description,
                    CategoryName = p.Category.CategoryName,
                    MeasureOfScaleName = p.MeasureOfScale.MeasureOfScale
                });

            return View(products.First(p => p.ProductId == id));
            /*var query = from c in _context.Categorys
                        join p in _context.Products on c.CategoryId equals p.CategoryId where p.ProductId == id
                        join m in _context.MeasuresOfScales on p.MeasureOfScaleId equals m.MeasureOfScaleId where p.ProductId == id
                        join i in _context.Images on p.ProductId equals i.ProductId  where p.ProductId == id
                        select new
                        {
                            CategoryName = c.CategoryName,
                            ProductName = p.ProductName,
                            ProductId = p.ProductId,
                            Price = p.Price,
                            Instock = p.Instock,
                            InstockQty = p.InstockQty,
                            MeasureOfScale = m.MeasureOfScale,
                            NewOrUsed = p.NewOrUsed,
                            Date = p.SignupDate
                        };
            var stringBuilder = new StringBuilder();

            var sd = _context.Images.FirstOrDefault(x => x.ProductId == id && x.ImageId == 1);
            productViewModel.Image1 = sd.Image1;

            sd = _context.Images.FirstOrDefault(x => x.ProductId == id && x.ImageId == 2);
            productViewModel.Image2 = sd.Image1;

            sd = _context.Images.FirstOrDefault(x => x.ProductId == id && x.ImageId == 3);
            productViewModel.Image3 = sd.Image1;

            sd = _context.Images.FirstOrDefault(x => x.ProductId == id && x.ImageId == 4);
            productViewModel.Image4 = sd.Image1;

            var final = query.Select(p => new ProductViewModel
           {
               ProductId = p.ProductId,
               CategoryName = p.CategoryName,
               ProductName = p.ProductName,
               Price = p.Price,
               InstockQty = p.InstockQty,
               MeasureOfScale = p.MeasureOfScale,
               NewOrUsed = p.NewOrUsed,
               SignupDate = p.Date,
               Image1 = productViewModel.Image1,
               Image2 = productViewModel.Image2,
               Image3 = productViewModel.Image3,
               Image4 = productViewModel.Image4,
           });

            return View(final.FirstOrDefault());*/

        }
    }
}
