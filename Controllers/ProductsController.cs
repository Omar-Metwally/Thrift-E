using Data_Layer;
using Infrastructure_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
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
        [Authorize(policy: "MustBeAdmin")]
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
        [Authorize(policy: "MustBeAdmin")]
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

        [Authorize(policy: "MustBeAdmin")]
        public IActionResult Upsert(int? id)
        {
            ViewBag.Brands = new SelectList(_context.Brands.ToList(), "BrandId", "BrandName");
            ViewBag.Categories = new SelectList(_context.Categorys.ToList(), "CategoryId", "CategoryName");
            ViewBag.MeasureOfScales = new SelectList(_context.MeasuresOfScales.ToList(), "MeasureOfScaleId", "MeasureOfScale");
            var entity = _unitOfWork.Products.Upsert(id);
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(policy: "MustBeAdmin")]
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

        public IActionResult Details(int? ProductId)
        {

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.MeasureOfScale)
                .Where(p => p.ProductId == ProductId)
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
                }).FirstOrDefault();

            return View(products);

        }

        public IActionResult Store()
        {
            ViewBag.Categories = new SelectList(_context.Categorys.ToList(), "CategoryId", "CategoryName");
            ViewBag.MeasureOfScales = new SelectList(_context.MeasuresOfScales.ToList(), "MeasureOfScaleId", "MeasureOfScale");
            ViewBag.Brands = new SelectList(_context.Brands.ToList(), "BrandId", "BrandName");

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.MeasureOfScale)
                .Include(p => p.Brand)
                .OrderBy(p => p.Price)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Size = p.Size,
                    BrandName = p.Brand.BrandName,
                    BrandId = p.BrandId,
                    Image1 = p.Image1,
                    CategoryName = p.Category.CategoryName,
                    MeasureOfScaleName = p.MeasureOfScale.MeasureOfScale,
                    Description = p.Description,
                })
                .ToList();

            return View(products);

        }

        public IActionResult StoreFilter(double? lowPrice, double? highPrice, int? categoryId, int? brandId, string? size)
        {
            ViewBag.Categories = new SelectList(_context.Categorys.ToList(), "CategoryId", "CategoryName");
            ViewBag.MeasureOfScales = new SelectList(_context.MeasuresOfScales.ToList(), "MeasureOfScaleId", "MeasureOfScale");
            ViewBag.Brands = new SelectList(_context.MeasuresOfScales.ToList(), "BrandId", "BrandName");

            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.MeasureOfScale)
                .Include(p => p.Brand);


            if (lowPrice != null && highPrice != null)
            {
                productsQuery = _context.Products.Where(p => p.Price >= lowPrice.Value && p.Price <= highPrice.Value)
                    .Include(p => p.MeasureOfScale)
                    .Include(p => p.Brand);
            }

            if (categoryId != null)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId).Include(p => p.MeasureOfScale).Include(p => p.Brand);
            }

            if (brandId != null)
            {
                productsQuery = productsQuery.Where(p => p.BrandId == brandId).Include(p => p.MeasureOfScale).Include(p => p.Brand);
            }

            if (size != null)
            {
                productsQuery = productsQuery.Where(p => p.Size == size).Include(p => p.MeasureOfScale).Include(p => p.Brand);
            }

            var products = productsQuery.OrderBy(p => p.Price)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Size = p.Size,
                    BrandName = p.Brand.BrandName,
                    BrandId = p.BrandId,
                    Image1 = p.Image1,
                    CategoryName = p.Category.CategoryName,
                    MeasureOfScaleName = p.MeasureOfScale.MeasureOfScale,
                    Description = p.Description,
                })
                .ToList();

            return View("Store", products);
        }

        public IActionResult StoreSearch(string word)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.MeasureOfScale)
                .Include(p => p.Brand)
                .OrderBy(p => p.Price)
                .Where(p => p.ProductName.Contains(word))
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Size = p.Size,
                    BrandName = p.Brand.BrandName,
                    BrandId = p.BrandId,
                    Image1 = p.Image1,
                    CategoryName = p.Category.CategoryName,
                    MeasureOfScaleName = p.MeasureOfScale.MeasureOfScale,
                    Description = p.Description,
                })
                .ToList();

            return View("Store", products);
        }
        [Authorize(policy: "MustBeAdmin")]
        public IActionResult ReOrder()
        {
            ViewBag.Product = new SelectList(_context.Products.ToList(), "ProductId", "ProductName");
            return View();
        }
    }
}
