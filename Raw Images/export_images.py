# pre-req: pip install psd_tools
# run by calling `python export_images.py` from `Raw Images` dir
from psd_tools import PSDImage
import os
import glob

kritaExePath = "D:\\ProgramFiles\\Krita\\Krita (x64)\\bin\\krita.exe"
filesExcludedFromExport = ["david_ugly_mockups.kra", "menu_icon.kra", "obsticles.kra", "diver.kra", "sea_panda_header", "sea_panda_logo.kra", "whale_tail_app_icon", "sea_panda_logo_eyesopen", "sea_panda_logo_whitecircle", "sea_panda_logo_wideeyes"]
filesThatExportToSinglePng = ["powerup_circle", "speed_boost", "whale_tail"]
filesThatExportToSinglePngAndLayers = ["underwaterbackground_deepocean", "underwaterbackground_arctic", "underwaterbackgrounds"]
filesThatShouldntResizeLayers = ["gameplay_icons"] #TODO hook this up, fill this in
#TODO could improve by adding recursive layer group handling -- skipping for now because only 1 such file (diver.kra)

kritaFiles = glob.glob('./**/*.kra', recursive=True)
kritaFiles = list(filter(lambda f: all(ef not in f for ef in filesExcludedFromExport), kritaFiles))
kritaFiles = list(filter(lambda f: "kra-autosave" not in f, kritaFiles))
# kritaFiles = list(filter(lambda f: "yacht" in f, kritaFiles))
print("The following files will be exported to PNGs:")
print(kritaFiles)
print()
for kraFilePath in kritaFiles:
    psdFilePath = kraFilePath.replace('.kra', '.psd')
    print(f'Creating {psdFilePath} from {kraFilePath}')
    os.system(f'"{kritaExePath}" {kraFilePath} --export --export-filename {psdFilePath}')
    filePathAndFile = os.path.split(psdFilePath)
    originalFileBaseName = filePathAndFile[1].replace('.psd', '')    
    psd = PSDImage.open(psdFilePath)
    
    shouldNotResize = any(nonResizeFile in psdFilePath for nonResizeFile in filesThatShouldntResizeLayers)
    if shouldNotResize:
        print(f'{psdFilePath} layers will not be resized, because that file was found in filesThatShouldntResizeLayers list')
    numLayers = sum(1 for layer in psd)
    numVisibleLayers = sum(1 for layer in psd if layer.visible)
    isSingleImage = any(singlePngFile in psdFilePath for singlePngFile in filesThatExportToSinglePng) or numLayers == 1
    isSingleImageAndLayers = any(singlePngFile in psdFilePath for singlePngFile in filesThatExportToSinglePngAndLayers) or numLayers == 1

    if isSingleImage:
        filename = f'..\Images\{originalFileBaseName}.png'.replace('\x00', '')
        print(f'Exporting single image {filename}')
        psd.composite().save(filename)
    else:
        if isSingleImageAndLayers:
            filename = f'..\Images\{originalFileBaseName}.png'.replace('\x00', '')
            print(f'Exporting single image {filename}')
            psd.composite().save(filename)
        for layer in psd:
            if (layer.visible):
                pathToImagesFolder = os.path.relpath('..\Images', filePathAndFile[0])
                path = f'{filePathAndFile[0]}\{pathToImagesFolder}\{filePathAndFile[0]}\{originalFileBaseName}\\'
                if (not os.path.exists(path)):
                    os.makedirs(path)
                filename = f'{path}{layer.name}.png'.replace('\x00', '')
                print(f'{layer.name} is visible, exporting to {filename} with size {psd.size}')
                if shouldNotResize:
                    composedLayer = layer.composite()
                else:
                    composedLayer = layer.composite(viewport=(0,0,psd.size[0],psd.size[1]))
                    #print(f'left: {layer.left}, top: {layer.top}, bbox: {layer.bbox}')
                    #composedLayer = layer.composite(viewport=(layer.left,layer.top,layer.left+layer.size[0],layer.top+layer.size[1]))
                composedLayer.save(filename)
            else:
                print(f'{layer.name} is not visible, skipping export')
    print(f'Removing {psdFilePath}')
    os.remove(psdFilePath)
    print()